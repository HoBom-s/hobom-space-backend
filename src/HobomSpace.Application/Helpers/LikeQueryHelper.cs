namespace HobomSpace.Application.Helpers;

public static class LikeQueryHelper
{
    public static string EscapeLikePattern(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }
}
