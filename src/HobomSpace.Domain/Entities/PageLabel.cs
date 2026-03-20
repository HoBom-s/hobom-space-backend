namespace HobomSpace.Domain.Entities;

/// <summary>
/// Page와 Label 간의 다대다 관계를 나타내는 조인 엔티티.
/// </summary>
public sealed class PageLabel
{
    public long Id { get; private set; }
    public long PageId { get; private set; }
    public long LabelId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PageLabel() { }

    /// <summary>페이지에 라벨을 부착한다.</summary>
    public static PageLabel Create(long pageId, long labelId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(labelId);

        return new PageLabel
        {
            PageId = pageId,
            LabelId = labelId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
