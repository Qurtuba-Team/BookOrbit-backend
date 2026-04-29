namespace BookOrbit.Application.SubcutaneousTests.Identity.Commands;

using BookOrbit.Application.Features.Identity.Commands.ChangePassword;
using BookOrbit.Application.Features.Identity.Commands.ConfirmEmail;
using BookOrbit.Application.Features.Identity.Commands.ResetPassowrd;
using BookOrbit.Application.Features.Identity.Commands.SendEmailConfirmation;
using BookOrbit.Application.Features.Identity.Commands.SendResetPassword;
using BookOrbit.Application.SubcutaneousTests.Identity.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class IdentityCommandsSubcutaneousTests
{
    [Fact]
    public async Task ChangePasswordCommand_ShouldReturnSuccess()
    {
        // Arrange
        var passwordService = new FakePasswordService();
        var handler = new ChangePasswordCommandHandler(
            passwordService,
            NullLogger<ChangePasswordCommandHandler>.Instance);

        var command = new ChangePasswordCommand("user@std.mans.edu.eg", "old", "new");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        passwordService.LastEmail.Should().Be("user@std.mans.edu.eg");
    }

    [Fact]
    public async Task ResetPasswordCommand_ShouldReturnUpdated()
    {
        // Arrange
        var passwordService = new FakePasswordService();
        var handler = new ResetPasswordCommandHandler(
            passwordService,
            NullLogger<ResetPasswordCommandHandler>.Instance);

        var command = new ResetPasswordCommand("user@std.mans.edu.eg", "token", "new");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        passwordService.LastResetToken.Should().Be("token");
    }

    [Fact]
    public async Task ConfirmEmailCommand_ShouldReturnUpdated()
    {
        // Arrange
        var emailConfirmationService = new FakeEmailConfirmationService();
        var handler = new ConfirmEmailCommandHandler(
            emailConfirmationService,
            NullLogger<ConfirmEmailCommandHandler>.Instance);

        var command = new ConfirmEmailCommand("user@std.mans.edu.eg", "token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        emailConfirmationService.LastToken.Should().Be("token");
    }

    [Fact]
    public async Task SendEmailConfirmationCommand_ShouldSendEmail()
    {
        // Arrange
        var emailConfirmationService = new FakeEmailConfirmationService();
        var emailService = new FakeEmailService();
        var emailFormatService = new FakeEmailFormatService();
        var handler = new SendEmailConfirmationCommandHandler(
            emailConfirmationService,
            NullLogger<SendEmailConfirmationCommandHandler>.Instance,
            emailService,
            new FakeRouteService(),
            emailFormatService);

        var command = new SendEmailConfirmationCommand("user@std.mans.edu.eg");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        emailService.LastEmail.Should().Be("user@std.mans.edu.eg");
        emailService.LastSubject.Should().Be("Confirm your email");
    }

    [Fact]
    public async Task SendResetPasswordCommand_ShouldSendEmail()
    {
        // Arrange
        var passwordService = new FakePasswordService();
        var emailService = new FakeEmailService();
        var emailFormatService = new FakeEmailFormatService();
        var handler = new SendResetPasswordCommandHandler(
            passwordService,
            NullLogger<SendResetPasswordCommandHandler>.Instance,
            new FakeRouteService(),
            emailFormatService,
            emailService);

        var command = new SendResetPasswordCommand("user@std.mans.edu.eg");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        emailService.LastEmail.Should().Be("user@std.mans.edu.eg");
        emailService.LastSubject.Should().Be("Reset your password");
    }
}
