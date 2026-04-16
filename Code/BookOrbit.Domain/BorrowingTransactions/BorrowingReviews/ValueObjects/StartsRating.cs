namespace BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.ValueObjects;

public record StartsRating(int Value) : ValueObject<int>(Value)
{
    public const int MinRating = 1;
    public const int MaxRating = 5;


    public static int Normalize(int value)
    {
        return value;
    }

    private static Result<int> Validate(int value)
    {
        if (value > MaxRating || value < MinRating)
            return BorrowingReviewErrors.InvalidRating;

        return value;
    }
    public static Result<StartsRating> Create(int? rate)
    {
        if (rate is null)
            return BorrowingReviewErrors.RatingRequired;

        var normalized = Normalize(rate.Value);
        var validationResult = Validate(normalized);

        if (validationResult.IsSuccess)
            return new StartsRating(validationResult.Value);

        return validationResult.Errors;
    }

}

