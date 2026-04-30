namespace BookOrbit.Application.SubcutaneousTests.Chatbot;

using BookOrbit.Application.Common.Interfaces;
using BookOrbit.Application.Common.Models;
using BookOrbit.Application.Features.Chatbot.Commands.SendChatMessage;
using FakeItEasy;
using FluentAssertions;

/// <summary>
/// Tests for <see cref="SendChatMessageCommandValidator"/>.
/// </summary>
public class SendChatMessageCommandValidatorTests
{
    private readonly SendChatMessageCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidMessage_PassesValidation()
    {
        var command = new SendChatMessageCommand("Find me a science book");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Validate_EmptyOrWhitespaceMessage_FailsValidation(string? message)
    {
        var command = new SendChatMessageCommand(message!);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SendChatMessageCommand.Message));
    }

    [Fact]
    public void Validate_MessageExceedsMaxLength_FailsValidation()
    {
        var tooLong = new string('A', SendChatMessageCommandValidator.MaxMessageLength + 1);
        var command = new SendChatMessageCommand(tooLong);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(SendChatMessageCommand.Message) &&
            e.ErrorMessage.Contains($"{SendChatMessageCommandValidator.MaxMessageLength}"));
    }

    [Fact]
    public void Validate_MessageAtExactMaxLength_PassesValidation()
    {
        var exactLength = new string('A', SendChatMessageCommandValidator.MaxMessageLength);
        var command = new SendChatMessageCommand(exactLength);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
