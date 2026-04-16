namespace BookOrbit.Domain.UnitTests.Books.ValueObjects;

using BookOrbit.Domain.Books.ValueObjects;
using BookOrbit.Domain.Books;
using FluentAssertions;
using Xunit;

public class BookTitleTests
{
    #region Create Method - Success Tests

    [Theory]
    [InlineData("The Great Gatsby")]
    [InlineData("To Kill a Mockingbird")]
    [InlineData("Pride and Prejudice")]
    [InlineData("abc")]
    [InlineData("The Lord of the Rings Fellowship of the Ring")]
    public void Create_WithValidTitle_ReturnsSuccessResult(string validTitle)
    {
        // Act
        var result = BookTitle.Create(validTitle);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(BookTitle.Normalize(validTitle));
    }

    [Theory]
    [InlineData("  The Great Gatsby  ")]
    [InlineData("\tTo Kill a Mockingbird\t")]
    [InlineData("Pride and Prejudice ")]
    [InlineData(" War and Peace ")]
    [InlineData("   Alice in Wonderland   ")]
    public void Create_WithTitleAndWhitespace_TrimsAndReturnsSuccess(string titleWithWhitespace)
    {
        // Act
        var result = BookTitle.Create(titleWithWhitespace);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(BookTitle.Normalize(titleWithWhitespace));
    }

    [Theory]
    [InlineData("aaa")]
    [InlineData("ab c")]
    [InlineData("TheBook")]
    public void Create_WithTitleBetweenMinAndMaxLength_ReturnsSuccess(string title)
    {
        // Act
        var result = BookTitle.Create(title);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithTitleExactlyAtMinLength_ReturnsSuccess()
    {
        // Arrange
        var titleAtMinLength = new string('A', BookTitle.MinLength);

        // Act
        var result = BookTitle.Create(titleAtMinLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().HaveLength(BookTitle.MinLength);
    }

    [Fact]
    public void Create_WithTitleExactlyAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        var titleAtMaxLength = new string('A', BookTitle.MaxLength);

        // Act
        var result = BookTitle.Create(titleAtMaxLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().HaveLength(BookTitle.MaxLength);
    }

    [Theory]
    [InlineData("The Great Gatsby")]
    [InlineData("To Kill a Mockingbird")]
    [InlineData("Pride and Prejudice and Zombies")]
    public void Create_WithTitleContainingSpaces_ReturnsSuccess(string title)
    {
        // Act
        var result = BookTitle.Create(title);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Contain(" ");
    }

    #endregion

    #region Create Method - Null/Empty Tests

    [Theory]
    [InlineData(null)]
    public void Create_WithNullTitle_ReturnsTitleRequiredError(string? nullTitle)
    {
        // Act
        var result = BookTitle.Create(nullTitle);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.TitleRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("  \t  \n  ")]
    public void Create_WithEmptyOrWhitespaceTitle_ReturnsTitleRequiredError(string title)
    {
        // Act
        var result = BookTitle.Create(title);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.TitleRequired);
    }

    #endregion

    #region Create Method - Invalid Length Tests

    [Fact]
    public void Create_WithTitleBelowMinLength_ReturnsInvalidTitleError()
    {
        // Arrange
        var shortTitle = new string('A', BookTitle.MinLength - 1);

        // Act
        var result = BookTitle.Create(shortTitle);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidTitle);
    }

    [Fact]
    public void Create_WithTitleExceedingMaxLength_ReturnsInvalidTitleError()
    {
        // Arrange
        var longTitle = new string('A', BookTitle.MaxLength + 1);

        // Act
        var result = BookTitle.Create(longTitle);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidTitle);
    }

    #endregion

    #region Create Method - Invalid Characters Tests

    [Theory]
    [InlineData("The Great Gatsby 123")]
    [InlineData("Book Title 001")]
    [InlineData("Title@123")]
    [InlineData("Book#1")]
    [InlineData("Title$Name1")]
    public void Create_WithTitleContainingNumbers_ReturnsInvalidTitleError(string title)
    {
        // Act
        var result = BookTitle.Create(title);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidTitle);
    }

    [Theory]
    [InlineData("The Great Gatsby@")]
    [InlineData("Book#Title")]
    [InlineData("Title$Name")]
    [InlineData("Book!Title")]
    [InlineData("Title&")]
    [InlineData("Book*")]
    [InlineData("Title-Name")]
    [InlineData("Book_")]
    [InlineData("Title.Name")]
    [InlineData("Book,")]
    public void Create_WithTitleContainingSpecialCharacters_ReturnsInvalidTitleError(string title)
    {
        // Act
        var result = BookTitle.Create(title);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidTitle);
    }

    [Theory]
    [InlineData("رواية عظيمة")]
    [InlineData("قصة الملك")]
    [InlineData("الكتاب المقدس")]
    [InlineData("ألف ليلة وليلة")]
    public void Create_WithArabicTitleName_ReturnsSuccessResult(string arabicTitle)
    {
        // Act
        var result = BookTitle.Create(arabicTitle);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(BookTitle.Normalize(arabicTitle));
    }

    [Theory]
    [InlineData("José's Book")]
    [InlineData("François Title")]
    [InlineData("Müller's Novel")]
    [InlineData("Björk Story")]
    public void Create_WithAccentedCharacters_ReturnsInvalidTitleError(string titleWithAccent)
    {
        // Act
        var result = BookTitle.Create(titleWithAccent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.InvalidTitle);
    }

    #endregion

    #region Normalize Method Tests

    [Theory]
    [InlineData("  The Great Gatsby  ", "The Great Gatsby")]
    [InlineData("\tTo Kill a Mockingbird\t", "To Kill a Mockingbird")]
    [InlineData("Pride and Prejudice ", "Pride and Prejudice")]
    [InlineData(" War and Peace ", "War and Peace")]
    [InlineData("   Multiple   Spaces   ", "Multiple   Spaces")]
    public void Normalize_WithVariousInputs_TrimsWhitespace(string input, string expected)
    {
        // Act
        var result = BookTitle.Normalize(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("The Great Gatsby")]
    [InlineData("To Kill a Mockingbird")]
    [InlineData("PRIDE AND PREJUDICE")]
    public void Normalize_PreservesOriginalCase(string input)
    {
        // Act
        var result = BookTitle.Normalize(input);

        // Assert
        result.Should().Be(input.Trim());
    }

    [Fact]
    public void Normalize_DoesNotConvertToLowerCase()
    {
        // Arrange
        var input = "THE GREAT GATSBY";

        // Act
        var result = BookTitle.Normalize(input);

        // Assert
        result.Should().Be("THE GREAT GATSBY");
        result.Should().NotBe("the great gatsby");
    }

    #endregion
}
