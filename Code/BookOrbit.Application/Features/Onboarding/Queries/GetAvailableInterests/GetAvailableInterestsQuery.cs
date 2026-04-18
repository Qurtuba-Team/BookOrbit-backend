namespace BookOrbit.Application.Features.Onboarding.Queries.GetAvailableInterests;

public sealed record GetAvailableInterestsQuery : IRequest<Result<List<InterestDto>>>;
