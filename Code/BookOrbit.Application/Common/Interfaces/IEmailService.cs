namespace BookOrbit.Application.Common.Interfaces;
public interface IEmailService
{
    Task<Result<Success>> SendEmailAsync(string emailAddress,
        string subject, string body);
}