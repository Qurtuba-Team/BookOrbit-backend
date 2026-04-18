namespace BookOrbit.Application.Features.Recommendations.Commands.RecordInteraction;

public class RecordInteractionCommandValidator : AbstractValidator<RecordInteractionCommand>
{
    public RecordInteractionCommandValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty()
            .WithMessage("BookId is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .When(x => x.Rating.HasValue)
            .WithMessage("Rating must be between 1 and 5.");
    }
}
