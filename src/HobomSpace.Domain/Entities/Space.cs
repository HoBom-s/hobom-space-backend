using HobomSpace.Domain.Common;
using HobomSpace.Domain.Events;
using HobomSpace.Domain.ValueObjects;

namespace HobomSpace.Domain.Entities;

/// <summary>
/// 문서 협업을 위한 최상위 컨테이너. 프로젝트 또는 팀 단위로 생성된다.
/// </summary>
public sealed class Space : AggregateRoot
{
    public long Id { get; private set; }

    /// <summary>고유 식별 키 (대문자, 최대 32자). URL 경로에 사용된다.</summary>
    public string Key { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Space() { }

    /// <summary>새 Space를 생성한다. <paramref name="key"/>는 자동으로 대문자 변환된다.</summary>
    public static Result<Space> Create(SpaceKey key, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Space>(DomainErrors.Space.NameEmpty);

        var now = DateTime.UtcNow;
        var space = new Space
        {
            Key = key,
            Name = name.Trim(),
            Description = description?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        space.RaiseDomainEvent(new SpaceCreatedEvent(space.Id, space.Key, space.Name, null));
        return space;
    }

    /// <summary>Space 이름과 설명을 변경한다.</summary>
    public Result Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(DomainErrors.Space.NameEmpty);

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
