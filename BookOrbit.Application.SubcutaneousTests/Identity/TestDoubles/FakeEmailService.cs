namespace BookOrbit.Application.SubcutaneousTests.Identity.TestDoubles;

using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Domain.Common.Results;

internal sealed class FakeEmailService : IEmailService
{
    public Result<Success> SendResult { get; set; } = Result.Success;

    public string? LastEmail { get; private set; }
    public string? LastSubject { get; private set; }
    public string? LastBody { get; private set; }

    public Task<Result<Success>> SendEmailAsync(string emailAddress, string subject, string body)
    {
        LastEmail = emailAddress;
        LastSubject = subject;
        LastBody = body;
        return Task.FromResult(SendResult);
    }
}
