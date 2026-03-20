using HobomSpace.Domain.Common;
using HobomSpace.Domain.ValueObjects;

namespace HobomSpace.Domain.Entities;

/// <summary>
/// Space 단위로 관리되는 라벨. 페이지에 태그처럼 부착할 수 있다.
/// </summary>
public sealed class Label
{
    public long Id { get; private set; }
    public long SpaceId { get; private set; }

    /// <summary>라벨 이름 (최대 50자, Space 내 고유).</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>HEX 색상 코드 (최대 7자, 예: #FF0000).</summary>
    public string Color { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    private Label() { }

    /// <summary>새 라벨을 생성한다.</summary>
    public static Result<Label> Create(long spaceId, string name, HexColor color)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Label>(new Error("Label.NameEmpty", "Name cannot be null or whitespace."));
        if (name.Trim().Length > 50)
            return Result.Failure<Label>(new Error("Label.NameTooLong", "Name must be 50 characters or less."));

        return new Label
        {
            SpaceId = spaceId,
            Name = name.Trim(),
            Color = color,
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>라벨 이름과 색상을 변경한다.</summary>
    public Result Update(string name, HexColor color)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("Label.NameEmpty", "Name cannot be null or whitespace."));
        if (name.Trim().Length > 50)
            return Result.Failure(new Error("Label.NameTooLong", "Name must be 50 characters or less."));

        Name = name.Trim();
        Color = color;
        return Result.Success();
    }
}
