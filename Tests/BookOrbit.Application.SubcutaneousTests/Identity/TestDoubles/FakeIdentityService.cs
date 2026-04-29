namespace BookOrbit.Application.SubcutaneousTests.Identity.TestDoubles;

using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Application.Features.Identity.Dtos;
using BookOrbit.Domain.Common.Results;

internal sealed class FakeIdentityService : IIdentityService
{
    public Result<AppUserDto> AuthenticateResult { get; set; }
        = new AppUserDto("Test", "user-1", "user@std.mans.edu.eg", [], [], true);

    public Result<AppUserDto> GetUserResult { get; set; }
        = new AppUserDto("Test", "user-1", "user@std.mans.edu.eg", [], [], true);

    public Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken ct = default)
        => Task.FromResult(AuthenticateResult);

    public Task<Result<AppUserDto>> GetUserByIdAsync(string id, CancellationToken ct = default)
        => Task.FromResult(GetUserResult);

    public Task<Result<string>> CreateStudent(string name, string email, string password, CancellationToken ct = default)
        => Task.FromResult<Result<string>>(Guid.NewGuid().ToString());

    public Task<Result<Deleted>> DeleteUserByIdAsync(string userId, CancellationToken ct = default)
        => Task.FromResult<Result<Deleted>>(Result.Deleted);

    public Task<bool> UserEmailExists(string email, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<string?> GetUserNameAsync(string userId, CancellationToken ct = default)
        => Task.FromResult<string?>("Test");

    public Task<Result<bool>> IsEmailConfirmedAsync(string userId, CancellationToken ct = default)
        => Task.FromResult<Result<bool>>(true);
}
