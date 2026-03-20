namespace HobomSpace.Application.Helpers;

/// <summary>라인 단위 diff 결과의 변경 유형.</summary>
public enum DiffType { UNCHANGED, ADDED, REMOVED }

/// <summary>diff 결과의 단일 라인 항목.</summary>
/// <param name="LineNumber">출력 라인 번호 (1-based).</param>
/// <param name="Type">변경 유형.</param>
/// <param name="Content">라인 내용.</param>
public record DiffEntry(int LineNumber, DiffType Type, string Content);

/// <summary>
/// LCS(Longest Common Subsequence) 기반 라인 단위 diff 유틸리티.
/// 공통 prefix/suffix를 먼저 제거하여 DP 범위를 최소화한다.
/// </summary>
public static class LineDiffHelper
{
    /// <summary>두 텍스트 간의 라인 단위 diff를 계산한다.</summary>
    public static List<DiffEntry> ComputeDiff(string oldText, string newText)
    {
        var oldLines = SplitLines(oldText);
        var newLines = SplitLines(newText);

        // 1) 공통 prefix/suffix 스킵 — 대부분 라인이 동일한 실제 편집 패턴에서 DP 범위를 줄임
        var prefix = 0;
        var minLen = Math.Min(oldLines.Length, newLines.Length);
        while (prefix < minLen && oldLines[prefix] == newLines[prefix])
            prefix++;

        var suffix = 0;
        while (suffix < minLen - prefix
               && oldLines[oldLines.Length - 1 - suffix] == newLines[newLines.Length - 1 - suffix])
        {
            suffix++;
        }

        var oldCore = oldLines.AsSpan(prefix, oldLines.Length - prefix - suffix);
        var newCore = newLines.AsSpan(prefix, newLines.Length - prefix - suffix);

        // 2) DP는 변경된 core 구간에만 적용
        var dp = BuildLcsTable(oldCore, newCore);
        var coreEntries = Backtrack(dp, oldCore, newCore);

        // 3) prefix + core + suffix 결합, LineNumber는 한 번만 할당
        var result = new List<DiffEntry>(prefix + coreEntries.Count + suffix);
        var line = 1;

        for (var i = 0; i < prefix; i++)
            result.Add(new DiffEntry(line++, DiffType.UNCHANGED, oldLines[i]));

        foreach (var entry in coreEntries)
            result.Add(new DiffEntry(line++, entry.Type, entry.Content));

        for (var i = 0; i < suffix; i++)
            result.Add(new DiffEntry(line++, DiffType.UNCHANGED, oldLines[oldLines.Length - suffix + i]));

        return result;
    }

    private static string[] SplitLines(string text)
        => string.IsNullOrEmpty(text) ? [] : text.Split('\n');

    /// <summary>LCS DP 테이블을 구축한다.</summary>
    private static int[,] BuildLcsTable(ReadOnlySpan<string> a, ReadOnlySpan<string> b)
    {
        var m = a.Length;
        var n = b.Length;
        var dp = new int[m + 1, n + 1];

        for (var i = 1; i <= m; i++)
        {
            for (var j = 1; j <= n; j++)
            {
                dp[i, j] = a[i - 1] == b[j - 1]
                    ? dp[i - 1, j - 1] + 1
                    : Math.Max(dp[i - 1, j], dp[i, j - 1]);
            }
        }

        return dp;
    }

    /// <summary>DP 테이블을 역추적하여 diff 엔트리를 생성한다.</summary>
    private static List<DiffEntry> Backtrack(int[,] dp, ReadOnlySpan<string> oldLines, ReadOnlySpan<string> newLines)
    {
        var entries = new List<DiffEntry>();
        var i = oldLines.Length;
        var j = newLines.Length;

        while (i > 0 || j > 0)
        {
            if (i > 0 && j > 0 && oldLines[i - 1] == newLines[j - 1])
            {
                entries.Add(new DiffEntry(0, DiffType.UNCHANGED, oldLines[i - 1]));
                i--; j--;
            }
            else if (j > 0 && (i == 0 || dp[i, j - 1] >= dp[i - 1, j]))
            {
                entries.Add(new DiffEntry(0, DiffType.ADDED, newLines[j - 1]));
                j--;
            }
            else
            {
                entries.Add(new DiffEntry(0, DiffType.REMOVED, oldLines[i - 1]));
                i--;
            }
        }

        entries.Reverse();
        return entries;
    }
}
