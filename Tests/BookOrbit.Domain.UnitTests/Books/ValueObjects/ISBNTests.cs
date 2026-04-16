namespace BookOrbit.Domain.UnitTests.Books.ValueObjects;

using BookOrbit.Domain.Books;
using BookOrbit.Domain.Books.ValueObjects;
using FluentAssertions;
using Xunit;

public class ISBNTests
{

    #region Create Method - Success Tests

    [Theory]
    [InlineData("9780131101928")]
    [InlineData("0131101928")]
    [InlineData("013110192X")]
    [InlineData("9780451524935")]
    public void Create_WithValidISBN_ReturnsSuccessResult(string validISBN)
    {
        // Act
        var result = ISBN.Create(validISBN);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(ISBN.Normalize(validISBN));
    }

    [Theory]
    [InlineData("978-0-13-110192-8")]
    [InlineData("0-13-110-192-8")]
    [InlineData("013-110-192-X")]
    [InlineData("978 0 13 110 192 8")]
    public void Create_WithISBNContainingDashesAndSpaces_NormalizesAndReturnsSuccess(string isbnWithFormatting)
    {
        // Act
        var result = ISBN.Create(isbnWithFormatting);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(ISBN.Normalize(isbnWithFormatting));
    }

    [Theory]
    [InlineData("  9780131101928  ")]
    [InlineData("\t0131101928\t")]
    [InlineData("013110192X ")]
    [InlineData(" 9780451524935 ")]
    public void Create_WithISBNAndWhitespace_NormalizesAndReturnsSuccess(string isbnWithWhitespace)
    {
        // Act
        var result = ISBN.Create(isbnWithWhitespace);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(ISBN.Normalize(isbnWithWhitespace));
    }

    [Fact]
    public void Create_WithISBN10ExactlyAtMinLength_ReturnsSuccess()
    {
        // Arrange
        var isbn10 = "0131101928";

        // Act
        var result = ISBN.Create(isbn10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().HaveLength(ISBN.MinLength);
    }

    [Fact]
    public void Create_WithISBN13ExactlyAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        var isbn13 = "9780131101928";

        // Act
        var result = ISBN.Create(isbn13);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().HaveLength(ISBN.MaxLength);
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("9876543210")]
    [InlineData("0000000000")]
    public void Create_WithValidISBN10Format_ReturnsSuccess(string isbn10)
    {
        // Act
        var result = ISBN.Create(isbn10);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("9780131101928")]
    [InlineData("9780451524935")]
    [InlineData("9781491927281")]
    public void Create_WithValidISBN13Format_ReturnsSuccess(string isbn13)
    {
        // Act
        var result = ISBN.Create(isbn13);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("123456789X")]
    [InlineData("809027341X")]
    public void Create_WithISBN10EndingInX_ReturnsSuccess(string isbn10WithX)
    {
        // Act
        var result = ISBN.Create(isbn10WithX);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Create Method - Null/Empty Tests

    [Theory]
    [InlineData(null)]
    public void Create_WithNullISBN_ReturnsISBNRequiredError(string? nullISBN)
    {
        // Act
        var result = ISBN.Create(nullISBN);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.ISBNRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("  \t  \n  ")]
    public void Create_WithEmptyOrWhitespaceISBN_ReturnsISBNRequiredError(string isbn)
    {
        // Act
        var result = ISBN.Create(isbn);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.ISBNRequired);
    }

    #endregion

    #region Create Method - Invalid Length Tests

    [Fact]
    public void Create_WithISBNBelowMinLength_ReturnsInvalidISBNError()
    {
        // Arrange
        var shortISBN = "123456789";  // 9 digits, less than min of 10

        // Act
        var result = ISBN.Create(shortISBN);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidISBN);
    }

    [Fact]
    public void Create_WithISBNExceedingMaxLength_ReturnsInvalidISBNError()
    {
        // Arrange
        var longISBN = "97801311019280";  // 14 digits, exceeds max of 13

        // Act
        var result = ISBN.Create(longISBN);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidISBN);
    }

    [Fact]
    public void Create_WithISBN11Digits_ReturnsInvalidISBNError()
    {
        // Arrange
        var isbn11 = "12345678901";

        // Act
        var result = ISBN.Create(isbn11);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidISBN);
    }

    [Fact]
    public void Create_WithISBN12Digits_ReturnsInvalidISBNError()
    {
        // Arrange
        var isbn12 = "123456789012";

        // Act
        var result = ISBN.Create(isbn12);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidISBN);
    }

    #endregion

    #region Create Method - Invalid Characters Tests

    [Theory]
    [InlineData("123456789a")]
    [InlineData("1234567890b")]
    [InlineData("9780131101a28")]
    [InlineData("978013110192c")]
    public void Create_WithISBNContainingLowercaseLetters_ReturnsInvalidISBNError(string isbn)
    {
        // Act
        var result = ISBN.Create(isbn);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidISBN);
    }

    [Theory]
    [InlineData("12345678@0")]
    [InlineData("978-0131#01928")]
    [InlineData("1234567890$")]
    [InlineData("978!0131101928")]
    [InlineData("978*0131101928")]
    [InlineData("978&0131101928")]
    public void Create_WithISBNContainingSpecialCharacters_ReturnsInvalidISBNError(string isbn)
    {
        // Act
        var result = ISBN.Create(isbn);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidISBN);
    }

    [Theory]
    [InlineData("ABCDEFGHIJ")]
    [InlineData("978ABCD01928")]
    [InlineData("ISBN0131101")]
    public void Create_WithISBNContainingOnlyLetters_ReturnsInvalidISBNError(string isbn)
    {
        // Act
        var result = ISBN.Create(isbn);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidISBN);
    }

    [Theory]
    [InlineData("123456789Y")]
    [InlineData("1234567890Y")]
    [InlineData("978Y131101928")]
    public void Create_WithISBNContainingCapitalLetterOtherThanX_ReturnsInvalidISBNError(string isbn)
    {
        // Act
        var result = ISBN.Create(isbn);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidISBN);
    }

    [Theory]
    [InlineData("123456789x")]
    [InlineData("97801311019x")]
    public void Create_WithISBNContainingLowercaseX_ReturnsInvalidISBNError(string isbn)
    {
        // Act
        var result = ISBN.Create(isbn);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidISBN);
    }

    #endregion

    #region Normalize Method Tests

    [Theory]
    [InlineData("978-0-13-110192-8", "9780131101928")]
    [InlineData("978 0 13 110 192 8", "9780131101928")]
    [InlineData("978-0 13-110 192-8", "9780131101928")]
    [InlineData("0-13-110-192-8", "0131101928")]
    [InlineData("013-110-192-X", "013110192X")]
    public void Normalize_WithFormattedISBN_RemovesDashesAndSpaces(string formattedISBN, string expected)
    {
        // Act
        var result = ISBN.Normalize(formattedISBN);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("  9780131101928  ", "9780131101928")]
    [InlineData("\t0131101928\t", "0131101928")]
    [InlineData("013110192X ", "013110192X")]
    [InlineData(" 9780451524935 ", "9780451524935")]
    public void Normalize_WithWhitespace_TrimsAndNormalizes(string isbnWithWhitespace, string expected)
    {
        // Act
        var result = ISBN.Normalize(isbnWithWhitespace);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("9780131101928")]
    [InlineData("0131101928")]
    [InlineData("013110192X")]
    public void Normalize_PreservesOriginalFormat(string isbn)
    {
        // Act
        var result = ISBN.Normalize(isbn);

        // Assert
        result.Should().Be(isbn.Trim());
    }

    [Fact]
    public void Normalize_ComplexFormatting_RemovesAllFormattingCharacters()
    {
        // Arrange
        var complexISBN = "  978 - 0 - 13 - 110192 - 8  ";

        // Act
        var result = ISBN.Normalize(complexISBN);

        // Assert
        result.Should().Be("9780131101928");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Create_WithISBNXInMiddle_ReturnsInvalidISBNError()
    {
        // Arrange
        var isbnWithXInMiddle = "013110X928";

        // Act
        var result = ISBN.Create(isbnWithXInMiddle);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidISBN);
    }

    [Fact]
    public void Create_WithISBN13EndingInX_ReturnsInvalidISBNError()
    {
        // Arrange
        var isbn13WithX = "978013110192X";

        // Act
        var result = ISBN.Create(isbn13WithX);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidISBN);
    }

    #endregion
}
