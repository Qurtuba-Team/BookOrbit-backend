namespace BookOrbit.Application.SubcutaneousTests.Identity.TestDoubles;

using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Application.Features.Identity.Dtos;
using BookOrbit.Domain.Common.Results;

internal sealed class FakePasswordService : IPasswordService
{
    public Result<Success> ChangePasswordResult { get; set; } = Result.Success;
    public Result<Success> ResetPasswordResult { get; set; } = Result.Success;
    public Result<ResetPasswordTokenDto> ResetTokenResult { get; set; }
        = new ResetPasswordTokenDto("user@std.mans.edu.eg", "token");

    public string? LastEmail { get; private set; }
    public string? LastOldPassword { get; private set; }
    public string? LastNewPassword { get; private set; }
    public string? LastResetToken { get; private set; }

    public Task<Result<Success>> ChangePasswordAsync(string email, string oldPassword, string newPassword)
    {
        LastEmail = email;
        LastOldPassword = oldPassword;
        LastNewPassword = newPassword;
        return Task.FromResult(ChangePasswordResult);
    }

    public Task<Result<Success>> ResetPasswordAsync(string email, string encodedToken, string newPassword, CancellationToken ct)
    {
        LastEmail = email;
        LastResetToken = encodedToken;
        LastNewPassword = newPassword;
        return Task.FromResult(ResetPasswordResult);
    }

    public Task<Result<ResetPasswordTokenDto>> GenerateResetPasswordTokenAsync(string email, CancellationToken ct)
    {
        LastEmail = email;
        return Task.FromResult(ResetTokenResult);
    }
}
