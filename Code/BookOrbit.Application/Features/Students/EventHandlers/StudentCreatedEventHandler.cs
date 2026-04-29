
namespace BookOrbit.Application.Features.Students.EventHandlers;
public class StudentCreatedEventHandler(ISender sender) : INotificationHandler<StudentCreatedEvent>
{
    public async Task Handle(StudentCreatedEvent notification, CancellationToken ct)
    {
        await sender.Send(
            new SendEmailConfirmationCommand(
                notification.UniversityMail.Value),
            ct);
    }
}
