namespace BookOrbit.Application.Common.Interfaces.OutboxMessages;
public class OutboxMessageRecord(string subject,
                        string emailAddress,
                        string emailFormat)
{
    public string Subject { get; set; } = subject;
    public string EmailAddress { get; set; } = emailAddress;
    public string EmailFormat { get; set; } = emailFormat;
}   