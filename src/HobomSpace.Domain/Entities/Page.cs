using HobomSpace.Domain.Common;
using HobomSpace.Domain.Events;

namespace HobomSpace.Domain.Entities;

/// <summary>
/// Space 내의 문서 페이지. 트리 구조(parent-child)를 지원하며 soft delete가 가능하다.
/// </summary>
public sealed class Page : AggregateRoot
{
    public long Id { get; private set; }
    public long SpaceId { get; private set; }

    /// <summary>부모 페이지 ID. <c>null</c>이면 루트 페이지.</summary>
    public long? ParentPageId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;

    /// <summary>같은 부모 아래에서의 정렬 순서.</summary>
    public int Position { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>Soft delete 시각. <c>null</c>이면 활성 상태.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>삭제를 수행한 사용자 식별자.</summary>
    public string? DeletedBy { get; private set; }

    private Page() { }

    /// <summary>새 페이지를 생성한다.</summary>
    public static Result<Page> Create(Space space, long? parentPageId, string title, string content, int position = 0, string? actorId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Page>(DomainErrors.Page.TitleEmpty);
        if (content is null)
            return Result.Failure<Page>(DomainErrors.Page.ContentNull);

        var now = DateTime.UtcNow;
        var page = new Page
        {
            SpaceId = space.Id,
            ParentPageId = parentPageId,
            Title = title.Trim(),
            Content = content,
            Position = Math.Max(0, position),
            CreatedAt = now,
            UpdatedAt = now,
        };

        page.RaiseDomainEvent(new PageCreatedEvent(page.Id, space.Id, space.Key, page.Title, actorId));
        return page;
    }

    /// <summary>페이지 제목, 본문, 정렬 순서를 변경한다.</summary>
    public Result Update(string title, string content, int? position)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(DomainErrors.Page.TitleEmpty);
        if (content is null)
            return Result.Failure(DomainErrors.Page.ContentNull);

        Title = title.Trim();
        Content = content;
        if (position.HasValue)
            Position = Math.Max(0, position.Value);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>페이지를 soft delete 처리한다. 30일 후 자동 영구 삭제 대상.</summary>
    public void SoftDelete(string spaceKey, string? actorId)
    {
        DeletedAt = DateTime.UtcNow;
        DeletedBy = actorId;
        RaiseDomainEvent(new PageDeletedEvent(Id, SpaceId, spaceKey, Title, actorId));
    }

    /// <summary>Soft delete된 페이지를 복원한다.</summary>
    public void Restore()
    {
        DeletedAt = null;
        DeletedBy = null;
    }

    /// <summary>페이지를 다른 Space 또는 다른 부모 아래로 이동한다.</summary>
    public Result MoveTo(Space targetSpace, Page? parentPage, string? actorId)
    {
        if (parentPage is not null && parentPage.SpaceId != targetSpace.Id)
            return Result.Failure(DomainErrors.Page.ParentNotInTargetSpace(parentPage.Id, targetSpace.Key));

        SpaceId = targetSpace.Id;
        ParentPageId = parentPage?.Id;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new PageMovedEvent(Id, targetSpace.Id, targetSpace.Key, Title, actorId));
        return Result.Success();
    }
}
