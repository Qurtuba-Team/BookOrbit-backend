namespace BookOrbit.Application.SubcutaneousTests.Identity.TestDoubles;

using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Application.Features.Identity.Dtos;
using BookOrbit.Domain.Common.Results;

internal sealed class FakeEmailConfirmationService : IEmailConfirmationService
{
    public Result<EmailConfirmationTokenDto> TokenResult { get; set; }
        = new EmailConfirmationTokenDto("encoded", "user@std.mans.edu.eg");

    public Result<Updated> ConfirmResult { get; set; } = Result.Updated;

    public string? LastEmail { get; private set; }
    public string? LastToken { get; private set; }

    public Task<Result<EmailConfirmationTokenDto>> GenerateEmailConfirmationTokenAsync(string email, CancellationToken ct = default)
    {
        LastEmail = email;
        return Task.FromResult(TokenResult);
    }

    public Task<Result<Updated>> ConfirmEmailAsync(string email, string encodedToken, CancellationToken ct = default)
    {
        LastEmail = email;
        LastToken = encodedToken;
        return Task.FromResult(ConfirmResult);
    }
}
