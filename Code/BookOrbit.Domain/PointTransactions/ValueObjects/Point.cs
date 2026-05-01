namespace BookOrbit.Domain.PointTransactions.ValueObjects;

public record Point(int Value) : ValueObject<int>(Value)
{
    public const int MinValue = int.MinValue;
    public const int MaxValue = int.MaxValue;

    public const int StudentInitialPoint = 3;

    public const int LendingRecordDefaultCost = 1;

    public const int DeliveringBookReward = 1;
    public const int ReturningBookReward = 1;
    public const int OverduePenalty = -2;
    public const int LostBookPenalty = -5;
    
    public const int OneStarReviewPenalty = -1;
    public const int TwoStarsReviewPenalty = -1;
    public const int ThreeStarsReviewReward = 0;
    public const int FourStarsReviewReward = 1;
    public const int FiveStarsReviewReward = 2;

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

    static public bool operator >(Point left, Point right) => left.Value > right.Value;
    static public bool operator <(Point left, Point right) => left.Value < right.Value; 
    static public Point operator +(Point left, Point right) => new Point(left.Value + right.Value);
    static public Point operator -(Point left, Point right) => new Point(left.Value - right.Value);

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