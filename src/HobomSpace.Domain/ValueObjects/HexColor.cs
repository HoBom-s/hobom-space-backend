using System.Text.RegularExpressions;
using HobomSpace.Domain.Common;

namespace HobomSpace.Domain.ValueObjects;

public sealed partial class HexColor : IEquatable<HexColor>
{
    private static readonly Regex Pattern = HexColorRegex();

    public string Value { get; }

    private HexColor(string value) => Value = value;

    public static Result<HexColor> Create(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return Result.Failure<HexColor>(new Error("HexColor.Empty", "Color cannot be null or whitespace."));

        var trimmed = color.Trim();
        if (!Pattern.IsMatch(trimmed))
            return Result.Failure<HexColor>(new Error("HexColor.Invalid", $"'{trimmed}' is not a valid hex color."));

        return new HexColor(trimmed);
    }

    public static implicit operator string(HexColor hexColor) => hexColor.Value;

    public override string ToString() => Value;
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override bool Equals(object? obj) => obj is HexColor other && Equals(other);
    public bool Equals(HexColor? other) => other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(@"^#[0-9A-Fa-f]{3,6}$")]
    private static partial Regex HexColorRegex();
}
