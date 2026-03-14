namespace HobomSpace.Application.Models;

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int Size)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Size);

    public static (int page, int size) Clamp(int page, int size, int maxSize = 100)
    {
        return (Math.Max(1, page), Math.Clamp(size, 1, maxSize));
    }
}
