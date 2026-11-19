namespace Gibo.Domain.Aggregates.UserAggregate;

public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public readonly record struct ConsentId(Guid Value)
{
    public static ConsentId New() => new(Guid.NewGuid());
    public static ConsentId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public readonly record struct PreferenceId(Guid Value)
{
    public static PreferenceId New() => new(Guid.NewGuid());
    public static PreferenceId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public readonly record struct GoalId(Guid Value)
{
    public static GoalId New() => new(Guid.NewGuid());
    public static GoalId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public sealed record Locale(string Code)
{
    public static readonly Locale DefaultEnglish = new("en");
    public static readonly Locale Arabic = new("ar");
    public static readonly Locale Hindi = new("hi");

    public static Locale From(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return DefaultEnglish;
        var normalized = code.Trim().ToLowerInvariant();
        return normalized switch
        {
            "ar" or "ar-ae" => Arabic,
            "hi" or "hi-in" => Hindi,
            _ => DefaultEnglish
        };
    }

    public override string ToString() => Code;
}
