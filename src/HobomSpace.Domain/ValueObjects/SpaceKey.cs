using HobomSpace.Domain.Common;

namespace HobomSpace.Domain.ValueObjects;

public sealed class SpaceKey : IEquatable<SpaceKey>
{
    public string Value { get; }

    private SpaceKey(string value) => Value = value;

    public static Result<SpaceKey> Create(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Result.Failure<SpaceKey>(DomainErrors.Space.KeyEmpty);

        var trimmed = key.Trim();
        if (trimmed.Length > 32)
            return Result.Failure<SpaceKey>(DomainErrors.Space.KeyTooLong);

        return new SpaceKey(trimmed.ToUpperInvariant());
    }

    public static implicit operator string(SpaceKey spaceKey) => spaceKey.Value;

    public override string ToString() => Value;
    public override int GetHashCode() => Value.GetHashCode();
    public override bool Equals(object? obj) => obj is SpaceKey other && Equals(other);
    public bool Equals(SpaceKey? other) => other is not null && Value == other.Value;
}
