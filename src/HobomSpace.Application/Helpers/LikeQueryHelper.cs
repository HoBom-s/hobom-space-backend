namespace HobomSpace.Application.Helpers;

/// <summary>SQL LIKE 패턴에서 사용되는 와일드카드 문자를 이스케이프하는 유틸리티.</summary>
public static class LikeQueryHelper
{
    /// <summary>입력 문자열에서 <c>%</c>, <c>_</c>, <c>\</c>를 이스케이프한다.</summary>
    public static string EscapeLikePattern(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }
}
