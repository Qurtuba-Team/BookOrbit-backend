namespace BookOrbit.Application.Features.Onboarding.Queries.GetAvailableInterests;

internal sealed class GetAvailableInterestsQueryHandler(
    IAppDbContext context) : IRequestHandler<GetAvailableInterestsQuery, Result<List<InterestDto>>>
{
    public async Task<Result<List<InterestDto>>> Handle(GetAvailableInterestsQuery query, CancellationToken ct)
    {
        var interests = await context.Interests
            .OrderBy(i => i.Name)
            .Select(i => new InterestDto(i.Id, i.Name, i.Type))
            .ToListAsync(ct);

        return interests;
    }
}
