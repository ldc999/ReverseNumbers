using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services;

public class GenerateService : IGenerateService
{
    private readonly ISqlSugarClient _db;
    private readonly IConfigService _configService;
    private static readonly Random _rng = new();

    public GenerateService(ISqlSugarClient db, IConfigService configService)
    {
        _db = db;
        _configService = configService;
    }

    public async Task<GenerateResponseDto> GenerateRandomAsync(GenerateRequestDto request)
    {
        // Validate constraints
        if (request.RequiredFrontNumbers.Count > 5)
            throw new AppException("前区必选号码不能超过5个");
        if (request.RequiredBackNumbers.Count > 2)
            throw new AppException("后区必选号码不能超过2个");

        // Load all player numbers for this issue
        var playerNumbers = await _db.Queryable<PlayerNumber>()
            .Where(p => p.IssueId == request.IssueId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        // Pre-parse into arrays of HashSet<int>
        var frontSets = playerNumbers.Select(p => ParseNumbers(p.FrontNumbers)).ToArray();
        var backSets = playerNumbers.Select(p => ParseNumbers(p.BackNumbers)).ToArray();
        int count = frontSets.Length;

        // Load history records within time range for constraint check (if enabled)
        var histFrontSets = Array.Empty<HashSet<int>>();
        var histBackSets = Array.Empty<HashSet<int>>();
        if (request.EnableHistoryConstraint && request.HistoryConstraints != null && request.HistoryConstraints.Count > 0
            && request.HistoryStart.HasValue)
        {
            var query = _db.Queryable<SavedNumberHistory>();
            if (request.HistoryEnd.HasValue)
                query = query.Where(h => h.CreatedAt >= request.HistoryStart.Value && h.CreatedAt <= request.HistoryEnd.Value);
            else
                query = query.Where(h => h.CreatedAt >= request.HistoryStart.Value);
            var histories = await query.ToListAsync();
            histFrontSets = histories.Select(h => ParseNumbers(h.FrontNumbers)).ToArray();
            histBackSets = histories.Select(h => ParseNumbers(h.BackNumbers)).ToArray();
        }

        // Pre-compute auto-excluded back numbers from ticket constraints
        var autoExcludedBack = new List<int>();
        if (request.TicketConstraints != null)
        {
            foreach (var tc in request.TicketConstraints)
            {
                // If ALL constraints for this ticket cap back at 0 (且下限也为0), exclude its back pool
                if (tc.Constraints.Count > 0 && tc.Constraints.All(c => c.B <= 0 && c.BMin <= 0))
                {
                    autoExcludedBack.AddRange(tc.BackPool);
                }
            }
        }
        var effectiveExcludedBack = request.ExcludedBackNumbers.Concat(autoExcludedBack).Distinct().ToList();

        int maxIter = int.MaxValue;
        // Regeneration loop
        for (int iter = 1; iter <= maxIter; iter++)
        {
            List<int> genFront, genBack;
            try
            {
                genFront = GenerateUniqueRandom(5, 1, 35,
                    request.ExcludedFrontNumbers, request.RequiredFrontNumbers);
                genBack = GenerateUniqueRandom(2, 1, 12,
                    effectiveExcludedBack, request.RequiredBackNumbers);
            }
            catch (InvalidOperationException)
            {
                throw new AppException("排除和必选条件冲突，无法生成号码");
            }

            if (IsValid(genFront, genBack, frontSets, backSets,
                        request.MaxFrontHighlight, request.MaxBackHighlight, count,
                        request.HighlightConstraints) &&
                CheckTicketConstraints(genFront, genBack, request.TicketConstraints) &&
                CheckHistoryConstraints(genFront, genBack, histFrontSets, histBackSets, request.HistoryConstraints))
            {
                var matchDetails = BuildMatchDetails(genFront, genBack, frontSets, backSets, count);
                return new GenerateResponseDto
                {
                    FrontNumbers = genFront,
                    BackNumbers = genBack,
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
        var playerNumbers = await _db.Queryable<PlayerNumber>()
            .Where(p => p.IssueId == request.IssueId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var frontSets = playerNumbers.Select(p => ParseNumbers(p.FrontNumbers)).ToArray();
        var backSets = playerNumbers.Select(p => ParseNumbers(p.BackNumbers)).ToArray();
        int count = frontSets.Length;

        var matchDetails = BuildMatchDetails(request.FrontNumbers, request.BackNumbers,
                                              frontSets, backSets, count);

        return new GenerateResponseDto
        {
            FrontNumbers = request.FrontNumbers,
            BackNumbers = request.BackNumbers,
            FrontMatchDetails = matchDetails.Front,
            BackMatchDetails = matchDetails.Back,
            IterationCount = 0
        };
    }

    // Parse "01,05,12,23,35" -> HashSet<int>{1,5,12,23,35}
    private HashSet<int> ParseNumbers(string csv)
    {
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(s => int.Parse(s.Trim()))
                  .ToHashSet();
    }

    // Generate N unique random integers in [min, max], sorted
    // Excluded numbers are removed from the pool entirely
    // Required numbers are guaranteed to be in the result
    private List<int> GenerateUniqueRandom(int count, int min, int max,
        List<int> excluded, List<int> required)
    {
        // Deduplicate
        var ex = excluded.Distinct().ToList();
        var req = required.Distinct().ToList();

        // Validate: required can't be in excluded
        var conflict = req.Intersect(ex).ToList();
        if (conflict.Count > 0)
            throw new InvalidOperationException($"号码冲突: {string.Join(",", conflict)} 同时在排除和必选中");

        // Validate: enough numbers available
        var pool = Enumerable.Range(min, max - min + 1).Except(ex).ToList();
        if (pool.Count < count)
            throw new InvalidOperationException($"排除后可用号码不足{count}个");

        // Validate: required numbers must be in the valid pool
        var invalidReq = req.Where(r => !pool.Contains(r)).ToList();
        if (invalidReq.Count > 0)
            throw new InvalidOperationException($"必选号码 {string.Join(",", invalidReq)} 不在可用范围内");

        // Randomly pick remaining numbers
        var remaining = pool.Except(req).ToList();
        Shuffle(remaining);
        var picked = remaining.Take(count - req.Count).ToList();

        return req.Concat(picked).OrderBy(x => x).ToList();
    }

    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    // Check against ALL player sets, fail early if any exceeds max
    // If highlightConstraints provided (OR), at least one must pass per player set
    private bool IsValid(
        List<int> genFront, List<int> genBack,
        HashSet<int>[] frontSets, HashSet<int>[] backSets,
        int maxFront, int maxBack, int count,
        List<TicketConstraintDto> highlightConstraints = null)
    {
        bool useOr = highlightConstraints != null && highlightConstraints.Count > 0;
        for (int i = 0; i < count; i++)
        {
            int fo = CountOverlap(genFront, frontSets[i]);
            int bo = CountOverlap(genBack, backSets[i]);
            if (useOr)
            {
                bool anyOk = false;
                foreach (var c in highlightConstraints)
                {
                    if (fo <= c.F && bo <= c.B) { anyOk = true; break; }
                }
                if (!anyOk) return false;
            }
            else
            {
                if (fo > maxFront || bo > maxBack) return false;
            }
        }
        return true;
    }

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
            // Ticket passes if ANY constraint is satisfied (OR)
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

    private static bool CheckHistoryConstraints(List<int> genFront, List<int> genBack,
        HashSet<int>[] histFrontSets, HashSet<int>[] histBackSets,
        List<TicketConstraintDto> constraints)
    {
        if (histFrontSets.Length == 0 || constraints == null || constraints.Count == 0) return true;
        for (int i = 0; i < histFrontSets.Length; i++)
        {
            int fo = genFront.Count(n => histFrontSets[i].Contains(n));
            int bo = genBack.Count(n => histBackSets[i].Contains(n));
            bool anyOk = false;
            foreach (var c in constraints)
            {
                if (fo >= c.FMin && fo <= c.F && bo >= c.BMin && bo <= c.B) { anyOk = true; break; }
            }
            if (!anyOk) return false;
        }
        return true;
    }

    private int CountOverlap(List<int> generated, HashSet<int> playerSet)
    {
        int count = 0;
        foreach (var num in generated)
            if (playerSet.Contains(num))
                count++;
        return count;
    }

    // Build match map: for each generated number, which player set indices contain it
    private (Dictionary<string, List<int>> Front, Dictionary<string, List<int>> Back)
        BuildMatchDetails(List<int> genFront, List<int> genBack,
                          HashSet<int>[] frontSets, HashSet<int>[] backSets, int count)
    {
        var frontMatches = new Dictionary<string, List<int>>();
        foreach (var num in genFront)
        {
            var key = num.ToString("D2");
            frontMatches[key] = new List<int>();
            for (int i = 0; i < count; i++)
                if (frontSets[i].Contains(num))
                    frontMatches[key].Add(i);
        }

        var backMatches = new Dictionary<string, List<int>>();
        foreach (var num in genBack)
        {
            var key = num.ToString("D2");
            backMatches[key] = new List<int>();
            for (int i = 0; i < count; i++)
                if (backSets[i].Contains(num))
                    backMatches[key].Add(i);
        }

        return (frontMatches, backMatches);
    }
}
