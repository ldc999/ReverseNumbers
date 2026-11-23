using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services.Ssq;

public interface ISsqGenerateService
{
    Task<GenerateResponseDto> GenerateRandomAsync(GenerateRequestDto request);
    Task<GenerateResponseDto> HighlightOnlyAsync(HighlightRequestDto request);
}

public class SsqGenerateService : ISsqGenerateService
{
    private readonly ISqlSugarClient _db;
    private static readonly Random _rng = new();

    // SSQ: Red 6/33, Blue 1/16
    private const int RedCount = 6, RedMin = 1, RedMax = 33;
    private const int BlueCount = 1, BlueMin = 1, BlueMax = 16;

    public SsqGenerateService(ISqlSugarClient db) => _db = db;

    public async Task<GenerateResponseDto> GenerateRandomAsync(GenerateRequestDto request)
    {
        if (request.RequiredFrontNumbers.Count > RedCount)
            throw new AppException($"红球必选号码不能超过{RedCount}个");
        if (request.RequiredBackNumbers.Count > BlueCount)
            throw new AppException($"蓝球必选号码不能超过{BlueCount}个");

        var playerNumbers = await _db.Queryable<SsqPlayerNumber>()
            .Where(p => p.IssueId == request.IssueId).OrderByDescending(p => p.CreatedAt).ToListAsync();

        var redSets = playerNumbers.Select(p => ParseNumbers(p.RedNumbers)).ToArray();
        var blueSets = playerNumbers.Select(p => ParseNumbers(p.BlueNumbers)).ToArray();
        int count = redSets.Length;

        // Load history within time range if enabled
        var histRedSets = Array.Empty<HashSet<int>>();
        var histBlueSets = Array.Empty<HashSet<int>>();
        if (request.EnableHistoryConstraint && request.HistoryConstraints != null && request.HistoryConstraints.Count > 0
            && request.HistoryStart.HasValue)
        {
            var q = _db.Queryable<SsqSavedNumberHistory>();
            q = request.HistoryEnd.HasValue
                ? q.Where(h => h.CreatedAt >= request.HistoryStart.Value && h.CreatedAt <= request.HistoryEnd.Value)
                : q.Where(h => h.CreatedAt >= request.HistoryStart.Value);
            var histories = await q.ToListAsync();
            histRedSets = histories.Select(h => ParseNumbers(h.RedNumbers)).ToArray();
            histBlueSets = histories.Select(h => ParseNumbers(h.BlueNumbers)).ToArray();
        }

        // Pre-compute auto-excluded blue numbers from ticket constraints
        var autoExcludedBlue = new List<int>();
        if (request.TicketConstraints != null)
        {
            foreach (var tc in request.TicketConstraints)
            {
                if (tc.Constraints.Count > 0 && tc.Constraints.All(c => c.B <= 0 && c.BMin <= 0))
                    autoExcludedBlue.AddRange(tc.BackPool);
            }
        }
        var effectiveExcludedBack = request.ExcludedBackNumbers.Concat(autoExcludedBlue).Distinct().ToList();

        int maxIter = int.MaxValue;

        for (int iter = 1; iter <= maxIter; iter++)
        {
            List<int> genRed, genBlue;
            try
            {
                genRed = GenerateUniqueRandom(RedCount, RedMin, RedMax,
                    request.ExcludedFrontNumbers, request.RequiredFrontNumbers);
                genBlue = GenerateUniqueRandom(BlueCount, BlueMin, BlueMax,
                    effectiveExcludedBack, request.RequiredBackNumbers);
            }
            catch (InvalidOperationException)
            {
                throw new AppException("排除和必选条件冲突，无法生成号码");
            }

            if (IsValid(genRed, genBlue, redSets, blueSets,
                        request.MaxFrontHighlight, request.MaxBackHighlight, count,
                        request.HighlightConstraints) &&
                CheckTicketConstraints(genRed, genBlue, request.TicketConstraints) &&
                CheckHistoryConstraints(genRed, genBlue, histRedSets, histBlueSets, request.HistoryConstraints))
            {
                var matchDetails = BuildMatchDetails(genRed, genBlue, redSets, blueSets, count);
                return new GenerateResponseDto
                {
                    FrontNumbers = genRed,
                    BackNumbers = genBlue,
                    FrontMatchDetails = matchDetails.Front,
                    BackMatchDetails = matchDetails.Back,
                    IterationCount = iter
                };
            }
        }
        throw new AppException("无法生成符合条件的号码，请尝试增大最大高亮数或减少玩家号码数量");
    }

    public async Task<GenerateResponseDto> HighlightOnlyAsync(HighlightRequestDto request)
    {
        var playerNumbers = await _db.Queryable<SsqPlayerNumber>()
            .Where(p => p.IssueId == request.IssueId).OrderByDescending(p => p.CreatedAt).ToListAsync();
        var redSets = playerNumbers.Select(p => ParseNumbers(p.RedNumbers)).ToArray();
        var blueSets = playerNumbers.Select(p => ParseNumbers(p.BlueNumbers)).ToArray();
        var matchDetails = BuildMatchDetails(request.FrontNumbers, request.BackNumbers, redSets, blueSets, redSets.Length);
        return new GenerateResponseDto
        {
            FrontNumbers = request.FrontNumbers,
            BackNumbers = request.BackNumbers,
            FrontMatchDetails = matchDetails.Front,
            BackMatchDetails = matchDetails.Back,
            IterationCount = 0
        };
    }

    private HashSet<int> ParseNumbers(string csv)
        => csv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();

    private List<int> GenerateUniqueRandom(int count, int min, int max, List<int> excluded, List<int> required)
    {
        var ex = excluded.Distinct().ToList();
        var req = required.Distinct().ToList();
        var conflict = req.Intersect(ex).ToList();
        if (conflict.Count > 0) throw new InvalidOperationException("冲突");
        var pool = Enumerable.Range(min, max - min + 1).Except(ex).ToList();
        if (pool.Count < count) throw new InvalidOperationException("号码不足");
        var invalidReq = req.Where(r => !pool.Contains(r)).ToList();
        if (invalidReq.Count > 0) throw new InvalidOperationException("必选号码不在范围");
        var remaining = pool.Except(req).ToList();
        Shuffle(remaining);
        return req.Concat(remaining.Take(count - req.Count)).OrderBy(x => x).ToList();
    }

    private void Shuffle<T>(IList<T> list) { int n = list.Count; while (n > 1) { n--; int k = _rng.Next(n + 1); (list[k], list[n]) = (list[n], list[k]); } }

    private bool IsValid(List<int> genRed, List<int> genBlue, HashSet<int>[] redSets, HashSet<int>[] blueSets, int maxRed, int maxBlue, int count,
        List<TicketConstraintDto> highlightConstraints = null)
    {
        bool useOr = highlightConstraints != null && highlightConstraints.Count > 0;
        for (int i = 0; i < count; i++)
        {
            int ro = CountOverlap(genRed, redSets[i]);
            int bo = CountOverlap(genBlue, blueSets[i]);
            if (useOr)
            {
                bool anyOk = false;
                foreach (var c in highlightConstraints) { if (ro <= c.F && bo <= c.B) { anyOk = true; break; } }
                if (!anyOk) return false;
            }
            else { if (ro > maxRed || bo > maxBlue) return false; }
        }
        return true;
    }

    private static bool CheckHistoryConstraints(List<int> genFront, List<int> genBack,
        HashSet<int>[] histRedSets, HashSet<int>[] histBlueSets, List<TicketConstraintDto> constraints)
    {
        if (histRedSets.Length == 0 || constraints == null || constraints.Count == 0) return true;
        for (int i = 0; i < histRedSets.Length; i++)
        {
            int ro = genFront.Count(n => histRedSets[i].Contains(n));
            int bo = genBack.Count(n => histBlueSets[i].Contains(n));
            bool anyOk = false;
            foreach (var c in constraints) { if (ro >= c.FMin && ro <= c.F && bo >= c.BMin && bo <= c.B) { anyOk = true; break; } }
            if (!anyOk) return false;
        }
        return true;
    }

    private int CountOverlap(List<int> gen, HashSet<int> set) { int c = 0; foreach (var n in gen) if (set.Contains(n)) c++; return c; }

    private static bool CheckTicketConstraints(List<int> genFront, List<int> genBack,
        List<TicketConstraintSetDto> tickets)
    {
        if (tickets == null || tickets.Count == 0) return true;
        foreach (var ticket in tickets)
        {
            var frontSet = new HashSet<int>(ticket.FrontPool);
            var backSet = new HashSet<int>(ticket.BackPool);
            int fOverlap = genFront.Count(n => frontSet.Contains(n));
            int bOverlap = genBack.Count(n => backSet.Contains(n));
            bool anyOk = false;
            foreach (var c in ticket.Constraints)
            {
                if (fOverlap >= c.FMin && fOverlap <= c.F &&
                    bOverlap >= c.BMin && bOverlap <= c.B) { anyOk = true; break; }
            }
            if (!anyOk) return false;
        }
        return true;
    }

    private (Dictionary<string, List<int>> Front, Dictionary<string, List<int>> Back)
        BuildMatchDetails(List<int> genRed, List<int> genBlue, HashSet<int>[] redSets, HashSet<int>[] blueSets, int count)
    {
        var redM = new Dictionary<string, List<int>>();
        foreach (var n in genRed) { var k = n.ToString("D2"); redM[k] = new List<int>(); for (int i = 0; i < count; i++) if (redSets[i].Contains(n)) redM[k].Add(i); }
        var blueM = new Dictionary<string, List<int>>();
        foreach (var n in genBlue) { var k = n.ToString("D2"); blueM[k] = new List<int>(); for (int i = 0; i < count; i++) if (blueSets[i].Contains(n)) blueM[k].Add(i); }
        return (redM, blueM);
    }
}
