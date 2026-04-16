namespace BookOrbit.Domain.UnitTests.ValueObjects;

using BookOrbit.Domain.Common.ValueObjects;
using FluentAssertions;
using Xunit;

public class TelegramUserIdTests
{
    #region Constants

    private const string NormalizedTelegramUserId = "abcd_1";

    #endregion

    #region Create Method - Success Tests

    [Theory]
    [InlineData("abcd_1")]
    [InlineData("abc12")]
    [InlineData("ab_c1d2")]
    public void Create_WithValidTelegramUserId_ReturnsSuccessResult(string validTelegramUserId)
    {
        // Act
        var result = TelegramUserId.Create(validTelegramUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(TelegramUserId.Normalize(validTelegramUserId));
    }

    [Theory]
    [InlineData("  AbCd_1  ")]
    [InlineData("\tABCD_1\t")]
    [InlineData("AbCd_1 ")]
    public void Create_WithWhitespaceAndCase_NormalizesAndReturnsSuccess(string input)
    {
        // Act
        var result = TelegramUserId.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(NormalizedTelegramUserId);
    }

    #endregion

    #region Create Method - Null/Empty Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("  \t  \n  ")]
    public void Create_WithNullOrEmptyTelegramUserId_ReturnsTelegramUserIdRequiredError(string? telegramUserId)
    {
        // Act
        var result = TelegramUserId.Create(telegramUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(TelegramUserIdErrors.TelegramUserIdRequired);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("ab1")] // less than 3 letters
    [InlineData("abcd!")] // invalid character
    [InlineData("ab.cd1")] // invalid character
    public void Create_WithInvalidPattern_ReturnsInvalidTelegramUserIdError(string invalidTelegramUserId)
    {
        // Act
        var result = TelegramUserId.Create(invalidTelegramUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(TelegramUserIdErrors.InvalidTelegramUserId);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Create_WithTelegramUserIdExactlyAtMinLength_ReturnsSuccess()
    {
        // Arrange
        var minLength = "abc" + new string('1', TelegramUserId.MinLength - 3);

        // Act
        var result = TelegramUserId.Create(minLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(TelegramUserId.Normalize(minLength));
    }

    [Fact]
    public void Create_WithTelegramUserIdExactlyAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        var maxLength = "abc" + new string('1', TelegramUserId.MaxLength - 3);

        // Act
        var result = TelegramUserId.Create(maxLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(TelegramUserId.Normalize(maxLength));
    }

    [Fact]
    public void Create_WithTelegramUserIdExceedingMaxLength_ReturnsInvalidTelegramUserIdError()
    {
        // Arrange
        var tooLong = "abc" + new string('1', TelegramUserId.MaxLength - 3 + 1);

        // Act
        var result = TelegramUserId.Create(tooLong);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(TelegramUserIdErrors.InvalidTelegramUserId);
    }

    #endregion

    #region Normalize Method Tests

    [Theory]
    [InlineData("  AbCd_1  ", NormalizedTelegramUserId)]
    [InlineData("ABCD_1", NormalizedTelegramUserId)]
    [InlineData("\tAbCd_1\t", NormalizedTelegramUserId)]
    public void Normalize_WithVariousInputs_ReturnsNormalizedTelegramUserId(string input, string expected)
    {
        // Act
        var result = TelegramUserId.Normalize(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
