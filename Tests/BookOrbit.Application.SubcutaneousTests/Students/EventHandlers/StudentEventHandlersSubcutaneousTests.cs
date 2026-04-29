namespace BookOrbit.Application.SubcutaneousTests.Students.EventHandlers;

using BookOrbit.Application.Features.Identity.Commands.SendEmailConfirmation;
using BookOrbit.Application.Features.Students.EventHandlers;
using BookOrbit.Application.SubcutaneousTests.Students.TestDoubles;
using BookOrbit.Domain.Students.DomainEvents;
using BookOrbit.Domain.Students.ValueObjects;
using FluentAssertions;
using Xunit;

public class StudentEventHandlersSubcutaneousTests
{
    [Fact]
    public async Task StudentCreatedEventHandler_ShouldSendEmailConfirmationCommand()
    {
        // Arrange
        var sender = new RecordingSender();
        var handler = new StudentCreatedEventHandler(sender);
        var mail = UniversityMail.Create("student@std.mans.edu.eg").Value;
        var notification = new StudentCreatedEvent(Guid.NewGuid(), mail);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        sender.LastRequest.Should().BeOfType<SendEmailConfirmationCommand>();
        ((SendEmailConfirmationCommand)sender.LastRequest!).Email.Should().Be(mail.Value);
    }
}
