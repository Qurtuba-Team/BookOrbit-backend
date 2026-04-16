namespace BookOrbit.Application.Common.Interfaces;
public interface IEmailService
{
    Task SendEmailAsync(string emailAddress,
        string subject, string body);
}