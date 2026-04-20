namespace BookOrbit.Application.Common.Interfaces;
public interface IPasswordService
{
    Task<Result<Success>> ChangePasswordAsync(string email,string oldPassword, string newPassword);
    Task<Result<Success>> ResetPasswordAsync(string email, string encodedToken, string newPassword, CancellationToken ct);
    Task<Result<ResetPasswordTokenDto>> GenerateResetPasswordTokenAsync(string email, CancellationToken ct );
}