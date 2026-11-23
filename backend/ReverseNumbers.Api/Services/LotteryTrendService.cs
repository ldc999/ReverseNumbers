using System.Text;
using System.Text.RegularExpressions;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services;

public interface ILotteryTrendService
{
    /// <summary>获取走势数据（本地缓存不足时自动抓取）</summary>
    Task<TrendResponseDto> GetTrendAsync(string game, int count, bool refresh);
}

public class LotteryTrendService : ILotteryTrendService
{
    private readonly ISqlSugarClient _db;

    private static readonly HttpClient _http = CreateHttpClient();

    private static HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All };
        return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
    }

    // 每行数据：<tr class="t_tr1"> ... </tr>
    private static readonly Regex RowRegex = new(@"<tr\s+class=[""']t_tr1[""'][^>]*>(.*?)</tr>",
        RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
    // HTML 注释中可能藏着 <td>，必须先剥离，否则会污染列索引
    private static readonly Regex CommentRegex = new(@"<!--[\s\S]*?-->", RegexOptions.Compiled);
    private static readonly Regex TdRegex = new(@"<td[^>]*>(.*?)</td>",
        RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex TagRegex = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex DateRegex = new(@"\d{4}-\d{2}-\d{2}", RegexOptions.Compiled);

    private const int MaxCount = 300;

    public LotteryTrendService(ISqlSugarClient db) => _db = db;

    public async Task<TrendResponseDto> GetTrendAsync(string game, int count, bool refresh)
    {
        game = NormalizeGame(game);
        count = Math.Clamp(count, 5, MaxCount);

        var (frontMax, backMax) = game == "ssq" ? (33, 16) : (35, 12);

        var cached = await _db.Queryable<LotteryDraw>()
            .Where(d => d.Game == game)
            .OrderBy(d => d.IssueNumber, OrderByType.Desc)
            .Take(MaxCount)
            .ToListAsync();

        // 缓存不足或强制刷新时抓取
        if (refresh || cached.Count < count)
        {
            var fetched = await FetchFrom500Async(game);
            await UpsertAsync(game, fetched);
            cached = await _db.Queryable<LotteryDraw>()
                .Where(d => d.Game == game)
                .OrderBy(d => d.IssueNumber, OrderByType.Desc)
                .Take(MaxCount)
                .ToListAsync();
        }

        return new TrendResponseDto
        {
            Game = game,
            FrontMax = frontMax,
            BackMax = backMax,
            CachedCount = cached.Count,
            Draws = cached.Take(count).Select(ToDto).ToList()
        };
    }

    private static string NormalizeGame(string game)
        => string.Equals(game, "ssq", StringComparison.OrdinalIgnoreCase) ? "ssq" : "dlt";

    /// <summary>抓取近两年开奖数据（500.com 数据图库）</summary>
    private async Task<List<LotteryDrawDto>> FetchFrom500Async(string game)
    {
        var yy = DateTime.Now.Year % 100;
        var start = $"{yy - 1:D2}001";
        var end = $"{yy:D2}999";
        var url = $"https://datachart.500.com/{game}/history/newinc/history.php?start={start}&end={end}";

        string html;
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0 Safari/537.36");
            req.Headers.TryAddWithoutValidation("Referer", $"https://datachart.500.com/{game}/");
            using var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            var bytes = await resp.Content.ReadAsByteArrayAsync();
            // 页面为 GBK，但本次只需提取期号/号码/日期等 ASCII 内容，
            // 按 UTF-8 解码不会破坏 ASCII 片段（GBK 前导字节不会被 ASCII 数字当作续字节）。
            html = Encoding.UTF8.GetString(bytes);
        }
        catch (Exception ex)
        {
            throw new AppException($"获取 500.com 开奖数据失败：{ex.Message}");
        }

        var list = new List<LotteryDrawDto>();
        html = CommentRegex.Replace(html, "");
        foreach (Match row in RowRegex.Matches(html))
        {
            var cells = new List<string>();
            foreach (Match td in TdRegex.Matches(row.Groups[1].Value))
                cells.Add(TagRegex.Replace(td.Groups[1].Value, "").Replace("&nbsp;", "").Trim());

            // 形态：[0]=期号 [1..n]=前区/红球 [n+1..]=后区/蓝球 ...
            if (cells.Count < 3) continue;
            if (!Regex.IsMatch(cells[0], @"^\d{5}$")) continue;

            var frontCount = game == "ssq" ? 6 : 5;
            var backCount = game == "ssq" ? 1 : 2;
            if (cells.Count < 1 + frontCount + backCount) continue;

            var front = new List<int>();
            for (int i = 1; i <= frontCount; i++)
                if (int.TryParse(cells[i], out var f)) front.Add(f);
            if (front.Count != frontCount) continue;

            var back = new List<int>();
            for (int i = 1 + frontCount; i <= frontCount + backCount; i++)
                if (int.TryParse(cells[i], out var b)) back.Add(b);
            if (back.Count != backCount) continue;

            var date = cells.Select(c => DateRegex.Match(c)).FirstOrDefault(m => m.Success)?.Value;

            list.Add(new LotteryDrawDto
            {
                IssueNumber = cells[0],
                FrontNumbers = front,
                BackNumbers = back,
                DrawDate = date
            });
        }

        if (list.Count == 0)
            throw new AppException("未能从 500.com 解析到开奖数据，请稍后重试");

        return list;
    }

    /// <summary>按期号覆盖写入</summary>
    private async Task UpsertAsync(string game, List<LotteryDrawDto> draws)
    {
        if (draws.Count == 0) return;

        var issues = draws.Select(d => d.IssueNumber).ToList();
        var existing = await _db.Queryable<LotteryDraw>()
            .Where(d => d.Game == game && issues.Contains(d.IssueNumber))
            .ToListAsync();
        var existMap = existing.ToDictionary(d => d.IssueNumber);

        var toInsert = new List<LotteryDraw>();
        var toUpdate = new List<LotteryDraw>();

        foreach (var d in draws)
        {
            var front = string.Join(",", d.FrontNumbers.Select(n => n.ToString("D2")));
            var back = string.Join(",", d.BackNumbers.Select(n => n.ToString("D2")));

            if (existMap.TryGetValue(d.IssueNumber, out var e))
            {
                if (e.FrontNumbers != front || e.BackNumbers != back || e.DrawDate != d.DrawDate)
                {
                    e.FrontNumbers = front;
                    e.BackNumbers = back;
                    e.DrawDate = d.DrawDate;
                    toUpdate.Add(e);
                }
            }
            else
            {
                toInsert.Add(new LotteryDraw
                {
                    Game = game,
                    IssueNumber = d.IssueNumber,
                    FrontNumbers = front,
                    BackNumbers = back,
                    DrawDate = d.DrawDate,
                    CreatedAt = DateTime.Now
                });
            }
        }

        if (toInsert.Count > 0) await _db.Insertable(toInsert).ExecuteCommandAsync();
        if (toUpdate.Count > 0) await _db.Updateable(toUpdate).ExecuteCommandAsync();
    }

    private static LotteryDrawDto ToDto(LotteryDraw e) => new()
    {
        IssueNumber = e.IssueNumber,
        FrontNumbers = e.FrontNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList(),
        BackNumbers = e.BackNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList(),
        DrawDate = e.DrawDate
    };
}
