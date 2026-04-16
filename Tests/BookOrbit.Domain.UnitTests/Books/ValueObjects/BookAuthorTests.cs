namespace BookOrbit.Domain.UnitTests.Books.ValueObjects;

using BookOrbit.Domain.Books.ValueObjects;
using BookOrbit.Domain.Books;
using FluentAssertions;
using Xunit;

public class BookAuthorTests
{
    #region Create Method - Success Tests

    [Theory]
    [InlineData("Ahmed")]
    [InlineData("John Smith")]
    [InlineData("Mary Jane Watson")]
    [InlineData("abc")]
    [InlineData("Alexander Christopher Madison")]
    public void Create_WithValidAuthor_ReturnsSuccessResult(string validAuthor)
    {
        // Act
        var result = BookAuthor.Create(validAuthor);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(BookAuthor.Normalize(validAuthor));
    }

    [Theory]
    [InlineData("  Ahmed  ")]
    [InlineData("\tJohn Smith\t")]
    [InlineData("Mary Jane Watson ")]
    [InlineData(" Robert Charles ")]
    [InlineData("   Alice   ")]
    public void Create_WithAuthorAndWhitespace_TrimsAndReturnsSuccess(string authorWithWhitespace)
    {
        // Act
        var result = BookAuthor.Create(authorWithWhitespace);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(BookAuthor.Normalize(authorWithWhitespace));
    }

    [Theory]
    [InlineData("aaa")]
    [InlineData("ab c")]
    [InlineData("Alexander")]
    public void Create_WithAuthorBetweenMinAndMaxLength_ReturnsSuccess(string author)
    {
        // Act
        var result = BookAuthor.Create(author);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithAuthorExactlyAtMinLength_ReturnsSuccess()
    {
        // Arrange
        var authorAtMinLength = new string('A', BookAuthor.MinLength);

        // Act
        var result = BookAuthor.Create(authorAtMinLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().HaveLength(BookAuthor.MinLength);
    }

    [Fact]
    public void Create_WithAuthorExactlyAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        var authorAtMaxLength = new string('A', BookAuthor.MaxLength);

        // Act
        var result = BookAuthor.Create(authorAtMaxLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().HaveLength(BookAuthor.MaxLength);
    }

    [Theory]
    [InlineData("John Smith")]
    [InlineData("Mary Anna Brown")]
    [InlineData("Robert Charles George Adams")]
    public void Create_WithAuthorContainingSpaces_ReturnsSuccess(string author)
    {
        // Act
        var result = BookAuthor.Create(author);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Contain(" ");
    }

    #endregion

    #region Create Method - Null/Empty Tests

    [Theory]
    [InlineData(null)]
    public void Create_WithNullAuthor_ReturnsAuthorRequiredError(string? nullAuthor)
    {
        // Act
        var result = BookAuthor.Create(nullAuthor);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.AuthorRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("  \t  \n  ")]
    public void Create_WithEmptyOrWhitespaceAuthor_ReturnsAuthorRequiredError(string author)
    {
        // Act
        var result = BookAuthor.Create(author);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.AuthorRequired);
    }

    #endregion

    #region Create Method - Invalid Length Tests

    [Fact]
    public void Create_WithAuthorBelowMinLength_ReturnsInvalidAuthorError()
    {
        // Arrange
        var shortAuthor = new string('A', BookAuthor.MinLength - 1);

        // Act
        var result = BookAuthor.Create(shortAuthor);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidAuthor);
    }

    [Fact]
    public void Create_WithAuthorExceedingMaxLength_ReturnsInvalidAuthorError()
    {
        // Arrange
        var longAuthor = new string('A', BookAuthor.MaxLength + 1);

        // Act
        var result = BookAuthor.Create(longAuthor);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidAuthor);
    }

    #endregion

    #region Create Method - Invalid Characters Tests

    [Theory]
    [InlineData("Ahmed123")]
    [InlineData("John123Smith")]
    [InlineData("Author@123")]
    [InlineData("Writer#1")]
    [InlineData("Author$Name1")]
    public void Create_WithAuthorContainingNumbers_ReturnsInvalidAuthorError(string author)
    {
        // Act
        var result = BookAuthor.Create(author);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidAuthor);
    }

    [Theory]
    [InlineData("Ahmed@")]
    [InlineData("John#Smith")]
    [InlineData("Mary$Jane")]
    [InlineData("Author!Name")]
    [InlineData("Name&")]
    [InlineData("Writer*")]
    [InlineData("Author-Name")]
    [InlineData("Name_")]
    [InlineData("Author.Name")]
    [InlineData("Name,")]
    public void Create_WithAuthorContainingSpecialCharacters_ReturnsInvalidAuthorError(string author)
    {
        // Act
        var result = BookAuthor.Create(author);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidAuthor);
    }

    [Theory]
    [InlineData("أحمد")]
    [InlineData("محمد علي")]
    [InlineData("فاطمة")]
    [InlineData("عمر")]
    public void Create_WithArabicAuthorName_ReturnsSuccessResult(string arabicAuthor)
    {
        // Act
        var result = BookAuthor.Create(arabicAuthor);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(BookAuthor.Normalize(arabicAuthor));
    }

    [Theory]
    [InlineData("José")]
    [InlineData("François")]
    [InlineData("Müller")]
    [InlineData("Björk")]
    public void Create_WithAccentedCharacters_ReturnsInvalidAuthorError(string authorWithAccent)
    {
        // Act
        var result = BookAuthor.Create(authorWithAccent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidAuthor);
    }

    #endregion

    #region Normalize Method Tests

    [Theory]
    [InlineData("  Ahmed  ", "Ahmed")]
    [InlineData("\tJohn Smith\t", "John Smith")]
    [InlineData("Mary Jane Watson ", "Mary Jane Watson")]
    [InlineData(" Robert Charles ", "Robert Charles")]
    [InlineData("   Multiple   Spaces   ", "Multiple   Spaces")]
    public void Normalize_WithVariousInputs_TrimsWhitespace(string input, string expected)
    {
        // Act
        var result = BookAuthor.Normalize(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Ahmed")]
    [InlineData("John Smith")]
    [InlineData("MARY")]
    public void Normalize_PreservesOriginalCase(string input)
    {
        // Act
        var result = BookAuthor.Normalize(input);

        // Assert
        result.Should().Be(input.Trim());
    }

    [Fact]
    public void Normalize_DoesNotConvertToLowerCase()
    {
        // Arrange
        var input = "JOHN SMITH";

        // Act
        var result = BookAuthor.Normalize(input);

        // Assert
        result.Should().Be("JOHN SMITH");
        result.Should().NotBe("john smith");
    }

    #endregion
}
