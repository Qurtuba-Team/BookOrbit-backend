namespace BookOrbit.Domain.PointTransactions.ValueObjects;

public record Point : ValueObject<int>
{
    public const int MinValue = int.MinValue;
    public const int MaxValue = int.MaxValue;

    public static readonly Point StudentInitialPoint = new(3);

    public static readonly Point LendingRecordDefaultCost = new(1);

    public static readonly Point DeliveringBookReward = new(1);
    public static readonly Point ReturningBookReward = new(1);
    public static readonly Point OverduePenalty = new(2);
    public static readonly Point LostBookPenalty = new(5);

    public static readonly Point OneStarReviewPenalty = new(1);
    public static readonly Point TwoStarsReviewPenalty = new(1);
    public static readonly Point ThreeStarsReviewReward = new(0);
    public static readonly Point FourStarsReviewReward = new(1);
    public static readonly Point FiveStarsReviewReward = new(2);
    private Point(int Value) : base(Value)
    {
    }

    private Point() : base(0)
    {
    }

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

    static public bool operator >(Point left, Point right) => (left?.Value ?? 0) > (right?.Value ?? 0);
    static public bool operator <(Point left, Point right) => (left?.Value ?? 0) < (right?.Value ?? 0); 
    static public Point operator +(Point left, Point right) => new((left?.Value ?? 0) + (right?.Value ?? 0));
    static public Point operator -(Point left, Point right) => new((left?.Value ?? 0) - (right?.Value ?? 0));

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