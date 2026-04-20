namespace BookOrbit.Application.Common.Interfaces;
public interface IIdentityService
{
    Task<Result<AppUserDto>> AuthenticateAsync(string email,string password,CancellationToken ct = default);
    Task<Result<AppUserDto>> GetUserByIdAsync(string id, CancellationToken ct = default);
    Task<Result<string>> CreateStudent(string email,string password,CancellationToken ct = default);
    Task<Result<Deleted>> DeleteUserByIdAsync(string userId, CancellationToken ct = default);
    Task<bool> UserEmailExists(string email,CancellationToken ct = default);
    Task<string?> GetUserNameAsync(string userId,CancellationToken ct = default);
    Task<Result<bool>> IsEmailConfirmedAsync(string userId,CancellationToken ct = default);
}