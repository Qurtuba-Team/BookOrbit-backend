namespace BookOrbit.Application.Features.Recommendations.Queries.GetRecommendations;

public class GetRecommendationsQueryValidator : AbstractValidator<GetRecommendationsQuery>
{
    public GetRecommendationsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required to fetch personalised recommendations.");
    }
}
