namespace BookOrbit.Application.Common.Interfaces;
public interface IEmailConfirmationService
{
    Task<Result<EmailConfirmationTokenDto>> GenerateEmailConfirmationTokenAsync(string email, CancellationToken ct = default);
    Task<Result<Updated>> ConfirmEmailAsync(string email, string encodedToken, CancellationToken ct = default);
}