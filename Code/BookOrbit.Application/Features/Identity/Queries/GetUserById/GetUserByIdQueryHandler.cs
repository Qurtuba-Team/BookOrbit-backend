namespace BookOrbit.Application.Features.Identity.Queries.GetUserById;
public class GetUserByIdQueryHandler(
    IIdentityService identityService,
    ILogger<GetUserByIdQueryHandler> logger)
    : IRequestHandler<GetUserByIdQuery, Result<AppUserDto>>
{
    public async Task<Result<AppUserDto>> Handle(
        GetUserByIdQuery request,
        CancellationToken ct)
    {
        var userResutl = await identityService.GetUserByIdAsync(request.UserId!, ct);

        if (userResutl.IsFailure)
        {
            logger.LogError("Couldn't Find User with Id { UserId }{ErrorDetails}", request.UserId, userResutl.TopError.Description);
            return userResutl.Errors;
        }
        return userResutl.Value;
    }
}