using HobomSpace.Domain.Common;

namespace HobomSpace.Domain.Entities;

/// <summary>
/// 페이지 수정 전의 스냅샷. 버전 번호는 자동 증가한다.
/// </summary>
public sealed class PageVersion
{
    public long Id { get; private set; }
    public long PageId { get; private set; }

    /// <summary>1부터 시작하는 버전 번호.</summary>
    public int Version { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? EditedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PageVersion() { }

    /// <summary>
    /// 현재 페이지 상태를 스냅샷으로 저장한다.
    /// <paramref name="version"/>에 1을 더한 값이 새 버전 번호가 된다.
    /// </summary>
    public static Result<PageVersion> Create(long pageId, int version, string title, string content, string? editedBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<PageVersion>(new Error("PageVersion.TitleEmpty", "Title cannot be null or whitespace."));

        return new PageVersion
        {
            PageId = pageId,
            Version = version + 1,
            Title = title,
            Content = content,
            EditedBy = editedBy,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
