namespace BookOrbit.Application.SubcutaneousTests.Students.TestDoubles;

using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Application.Features.Identity.Dtos;
using BookOrbit.Domain.Common.Results;

internal sealed class FakeIdentityService : IIdentityService
{
    private readonly HashSet<string> emails = [];
    private readonly Dictionary<string, bool> emailConfirmed = new();

    public Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken ct = default)
        => Task.FromResult<Result<AppUserDto>>(Error.Failure("Auth.Invalid", "Authentication not supported in tests."));

    public Task<Result<AppUserDto>> GetUserByIdAsync(string id, CancellationToken ct = default)
        => Task.FromResult<Result<AppUserDto>>(Error.NotFound("User.NotFound", "User not found."));

    public Task<Result<string>> CreateStudent(string name, string email, string password, CancellationToken ct = default)
    {
        emails.Add(email);
        var userId = Guid.NewGuid().ToString();
        emailConfirmed[userId] = true;
        return Task.FromResult<Result<string>>(userId);
    }

    public Task<Result<Deleted>> DeleteUserByIdAsync(string userId, CancellationToken ct = default)
        => Task.FromResult<Result<Deleted>>(Result.Deleted);

    public Task<bool> UserEmailExists(string email, CancellationToken ct = default)
        => Task.FromResult(emails.Contains(email));

    public Task<string?> GetUserNameAsync(string userId, CancellationToken ct = default)
        => Task.FromResult<string?>("test-user");

    public Task<Result<bool>> IsEmailConfirmedAsync(string userId, CancellationToken ct = default)
    {
        var confirmed = emailConfirmed.TryGetValue(userId, out var value) && value;
        return Task.FromResult<Result<bool>>(confirmed);
    }

    public void SetEmailConfirmed(string userId, bool confirmed)
        => emailConfirmed[userId] = confirmed;
}
