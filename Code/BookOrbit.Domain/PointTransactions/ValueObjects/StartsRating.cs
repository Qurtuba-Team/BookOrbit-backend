namespace BookOrbit.Domain.PointTransactions.ValueObjects;

public record Point(int Value) : ValueObject<int>(Value)
{
    public const int MinValue = 1;
    public const int MaxValue = int.MaxValue;


    public static int Normalize(int value)
    {
        return value;
    }

    private static Result<int> Validate(int value)
    {
        if (value > MaxValue || value < MinValue)
            return PointErrors.InvalidPoint;

        return value;
    }
    public static Result<Point> Create(int? rate)
    {
        if (rate is null)
            return PointErrors.RequiredPoint;

        var normalized = Normalize(rate.Value);
        var validationResult = Validate(normalized);

        if (validationResult.IsSuccess)
            return new Point(validationResult.Value);

        return validationResult.Errors;
    }
}

static public class PointErrors
{
    private const string className = nameof(Point);

    public static readonly Error InvalidPoint = DomainCommonErrors.InvalidProp(
                className,
                "Value",
                "Point",
                "It must be greater than 0."
            );

    public static readonly Error RequiredPoint = DomainCommonErrors.RequiredProp(
                className,
                "Value",
                "Point"
            );
}