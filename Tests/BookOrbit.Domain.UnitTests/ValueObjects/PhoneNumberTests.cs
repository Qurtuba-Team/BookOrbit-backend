namespace BookOrbit.Domain.UnitTests.ValueObjects;

using BookOrbit.Domain.Common.ValueObjects;
using FluentAssertions;
using Xunit;

public class PhoneNumberTests
{
    #region Constants

    private const string NormalizedPhoneNumber = "201012345678";

    #endregion

    #region Create Method - Success Tests

    [Theory]
    [InlineData("201012345678")]
    [InlineData("01012345678")]
    [InlineData("  01012345678  ")]
    public void Create_WithValidPhoneNumber_ReturnsSuccessResult(string validPhoneNumber)
    {
        // Act
        var result = PhoneNumber.Create(validPhoneNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(PhoneNumber.Normalize(validPhoneNumber));
    }

    [Theory]
    [InlineData("+201012345678")]
    [InlineData("2010 123 45678")]
    [InlineData("2010-123-45678")]
    public void Create_WithFormattedPhoneNumber_NormalizesAndReturnsSuccess(string formattedPhoneNumber)
    {
        // Act
        var result = PhoneNumber.Create(formattedPhoneNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(NormalizedPhoneNumber);
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
    public void Create_WithNullOrEmptyPhoneNumber_ReturnsRequiredPhoneNumberError(string? phoneNumber)
    {
        // Act
        var result = PhoneNumber.Create(phoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PhoneNumberErrors.RequiredPhoneNumber);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("201912345678")] // 201 + 9 is not allowed by the regex
    [InlineData("20101234567a")] // non digit
    public void Create_WithInvalidPattern_ReturnsInvalidPhoneNumberError(string invalidPhoneNumber)
    {
        // Act
        var result = PhoneNumber.Create(invalidPhoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PhoneNumberErrors.InvalidPhoneNumber);
    }

    [Fact]
    public void Create_WithPhoneNumberShorterThanMinLength_ReturnsInvalidPhoneNumberError()
    {
        // Arrange
        var tooShort = new string('1', PhoneNumber.MinLength - 1);

        // Act
        var result = PhoneNumber.Create(tooShort);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PhoneNumberErrors.InvalidPhoneNumber);
    }

    [Fact]
    public void Create_WithPhoneNumberLongerThanMaxLength_ReturnsInvalidPhoneNumberError()
    {
        // Arrange
        var tooLong = new string('1', PhoneNumber.MaxLength + 1);

        // Act
        var result = PhoneNumber.Create(tooLong);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(PhoneNumberErrors.InvalidPhoneNumber);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Create_WithPhoneNumberExactlyAtMinLength_ReturnsSuccess()
    {
        // Arrange
        var minLengthNumber = "010" + new string('1', PhoneNumber.MinLength - 3);

        // Act
        var result = PhoneNumber.Create(minLengthNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(PhoneNumber.Normalize(minLengthNumber));
    }

    [Fact]
    public void Create_WithPhoneNumberExactlyAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        var maxLengthNumber = "201" + new string('1', PhoneNumber.MaxLength - 3);

        // Act
        var result = PhoneNumber.Create(maxLengthNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(PhoneNumber.Normalize(maxLengthNumber));
    }

    #endregion

    #region Normalize Method Tests

    [Theory]
    [InlineData("01012345678", NormalizedPhoneNumber)]
    [InlineData(" 01012345678 ", NormalizedPhoneNumber)]
    [InlineData("+201012345678", NormalizedPhoneNumber)]
    [InlineData("2010 123 45678", NormalizedPhoneNumber)]
    [InlineData("2010-123-45678", NormalizedPhoneNumber)]
    public void Normalize_WithVariousInputs_ReturnsNormalizedPhoneNumber(string input, string expected)
    {
        // Act
        var result = PhoneNumber.Normalize(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
