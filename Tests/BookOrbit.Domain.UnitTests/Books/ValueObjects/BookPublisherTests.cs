namespace BookOrbit.Domain.UnitTests.Books.ValueObjects;

using BookOrbit.Domain.Books.ValueObjects;
using BookOrbit.Domain.Books;
using FluentAssertions;
using Xunit;

public class BookPublisherTests
{
    #region Create Method - Success Tests

    [Theory]
    [InlineData("Penguin")]
    [InlineData("Oxford University Press")]
    [InlineData("Simon and Schuster")]
    [InlineData("abc")]
    [InlineData("Random House Publishing Group")]
    public void Create_WithValidPublisher_ReturnsSuccessResult(string validPublisher)
    {
        // Act
        var result = BookPublisher.Create(validPublisher);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(BookPublisher.Normalize(validPublisher));
    }

    [Theory]
    [InlineData("  Penguin  ")]
    [InlineData("\tOxford University Press\t")]
    [InlineData("Simon and Schuster ")]
    [InlineData(" Hachette Book Group ")]
    [InlineData("   Macmillan   ")]
    public void Create_WithPublisherAndWhitespace_TrimsAndReturnsSuccess(string publisherWithWhitespace)
    {
        // Act
        var result = BookPublisher.Create(publisherWithWhitespace);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(BookPublisher.Normalize(publisherWithWhitespace));
    }

    [Theory]
    [InlineData("aaa")]
    [InlineData("ab c")]
    [InlineData("Publisher")]
    public void Create_WithPublisherBetweenMinAndMaxLength_ReturnsSuccess(string publisher)
    {
        // Act
        var result = BookPublisher.Create(publisher);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithPublisherExactlyAtMinLength_ReturnsSuccess()
    {
        // Arrange
        var publisherAtMinLength = new string('A', BookPublisher.MinLength);

        // Act
        var result = BookPublisher.Create(publisherAtMinLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().HaveLength(BookPublisher.MinLength);
    }

    [Fact]
    public void Create_WithPublisherExactlyAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        var publisherAtMaxLength = new string('A', BookPublisher.MaxLength);

        // Act
        var result = BookPublisher.Create(publisherAtMaxLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().HaveLength(BookPublisher.MaxLength);
    }

    [Theory]
    [InlineData("Oxford University Press")]
    [InlineData("Simon and Schuster")]
    [InlineData("Random House Publishing Group")]
    public void Create_WithPublisherContainingSpaces_ReturnsSuccess(string publisher)
    {
        // Act
        var result = BookPublisher.Create(publisher);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Contain(" ");
    }

    #endregion

    #region Create Method - Null/Empty Tests

    [Theory]
    [InlineData(null)]
    public void Create_WithNullPublisher_ReturnsPublisherRequiredError(string? nullPublisher)
    {
        // Act
        var result = BookPublisher.Create(nullPublisher);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.PublisherRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("  \t  \n  ")]
    public void Create_WithEmptyOrWhitespacePublisher_ReturnsPublisherRequiredError(string publisher)
    {
        // Act
        var result = BookPublisher.Create(publisher);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.PublisherRequired);
    }

    #endregion

    #region Create Method - Invalid Length Tests

    [Fact]
    public void Create_WithPublisherBelowMinLength_ReturnsInvalidPublisherError()
    {
        // Arrange
        var shortPublisher = new string('A', BookPublisher.MinLength - 1);

        // Act
        var result = BookPublisher.Create(shortPublisher);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidPublisher);
    }

    [Fact]
    public void Create_WithPublisherExceedingMaxLength_ReturnsInvalidPublisherError()
    {
        // Arrange
        var longPublisher = new string('A', BookPublisher.MaxLength + 1);

        // Act
        var result = BookPublisher.Create(longPublisher);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidPublisher);
    }

    #endregion

    #region Create Method - Invalid Characters Tests

    [Theory]
    [InlineData("Penguin123")]
    [InlineData("Oxford123Press")]
    [InlineData("Publisher@123")]
    [InlineData("Press#1")]
    [InlineData("Publisher$Name1")]
    public void Create_WithPublisherContainingNumbers_ReturnsInvalidPublisherError(string publisher)
    {
        // Act
        var result = BookPublisher.Create(publisher);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidPublisher);
    }

    [Theory]
    [InlineData("Penguin@")]
    [InlineData("Oxford#Press")]
    [InlineData("Simon$Jane")]
    [InlineData("Publisher!Name")]
    [InlineData("Press&")]
    [InlineData("Publisher*")]
    [InlineData("Publisher-Name")]
    [InlineData("Press_")]
    [InlineData("Publisher.Name")]
    [InlineData("Press,")]
    public void Create_WithPublisherContainingSpecialCharacters_ReturnsInvalidPublisherError(string publisher)
    {
        // Act
        var result = BookPublisher.Create(publisher);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidPublisher);
    }

    [Theory]
    [InlineData("دار النشر")]
    [InlineData("دار الكتب المصرية")]
    [InlineData("مؤسسة الأهرام")]
    [InlineData("دار الروايات")]
    public void Create_WithArabicPublisherName_ReturnsSuccessResult(string arabicPublisher)
    {
        // Act
        var result = BookPublisher.Create(arabicPublisher);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(BookPublisher.Normalize(arabicPublisher));
    }

    [Theory]
    [InlineData("José Publishing")]
    [InlineData("François Press")]
    [InlineData("Müller Publishing")]
    [InlineData("Björk Press")]
    public void Create_WithAccentedCharacters_ReturnsInvalidPublisherError(string publisherWithAccent)
    {
        // Act
        var result = BookPublisher.Create(publisherWithAccent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidPublisher);
    }

    #endregion

    #region Normalize Method Tests

    [Theory]
    [InlineData("  Penguin  ", "Penguin")]
    [InlineData("\tOxford University Press\t", "Oxford University Press")]
    [InlineData("Simon and Schuster ", "Simon and Schuster")]
    [InlineData(" Random House ", "Random House")]
    [InlineData("   Multiple   Spaces   ", "Multiple   Spaces")]
    public void Normalize_WithVariousInputs_TrimsWhitespace(string input, string expected)
    {
        // Act
        var result = BookPublisher.Normalize(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Penguin")]
    [InlineData("Oxford University Press")]
    [InlineData("HACHETTE")]
    public void Normalize_PreservesOriginalCase(string input)
    {
        // Act
        var result = BookPublisher.Normalize(input);

        // Assert
        result.Should().Be(input.Trim());
    }

    [Fact]
    public void Normalize_DoesNotConvertToLowerCase()
    {
        // Arrange
        var input = "OXFORD UNIVERSITY PRESS";

        // Act
        var result = BookPublisher.Normalize(input);

        // Assert
        result.Should().Be("OXFORD UNIVERSITY PRESS");
        result.Should().NotBe("oxford university press");
    }

    #endregion
}
