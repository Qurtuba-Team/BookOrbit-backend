namespace BookOrbit.Domain.UnitTests.Students.ValueObjects;

using BookOrbit.Domain.Students.ValueObjects;
using BookOrbit.Domain.Students;

public class StudentNameTests
{
    #region Create Method - Success Tests

    [Theory]
    [InlineData("Ahmed")]
    [InlineData("John Doe")]
    [InlineData("Mary Jane Watson")]
    [InlineData("abc")]
    [InlineData("Alexander Christopher")]
    public void Create_WithValidName_ReturnsSuccessResult(string validName)
    {
        // Act
        var result = StudentName.Create(validName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(StudentName.Normalize(validName));
    }

    [Theory]
    [InlineData("  Ahmed  ")]
    [InlineData("\tJohn Doe\t")]
    [InlineData("Mary Jane Watson ")]
    [InlineData(" Robert Charles ")]
    [InlineData("  Alice  ")]
    public void Create_WithNameAndWhitespace_TrimsAndReturnsSuccess(string nameWithWhitespace)
    {
        // Act
        var result = StudentName.Create(nameWithWhitespace);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(StudentName.Normalize(nameWithWhitespace));
    }

    [Theory]
    [InlineData("aaa")]
    [InlineData("ab c")]
    [InlineData("Alexander")]
    public void Create_WithNameBetweenMinAndMaxLength_ReturnsSuccess(string name)
    {
        // Act
        var result = StudentName.Create(name);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNameExactlyAtMinLength_ReturnsSuccess()
    {
        // Arrange
        var nameAtMinLength = new string('A', StudentName.MinLength);

        // Act
        var result = StudentName.Create(nameAtMinLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().HaveLength(StudentName.MinLength);

    }

    [Fact]
    public void Create_WithNameExactlyAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        var nameAtMaxLength = new string('A', StudentName.MaxLength);

        // Act
        var result = StudentName.Create(nameAtMaxLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().HaveLength(StudentName.MaxLength);
    }

    [Theory]
    [InlineData("John Smith")]
    [InlineData("Mary Anna")]
    [InlineData("Robert Charles George")]
    public void Create_WithNameContainingSpaces_ReturnsSuccess(string name)
    {
        // Act
        var result = StudentName.Create(name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Contain(" ");
    }

    #endregion

    #region Create Method - Null/Empty Tests

    [Theory]
    [InlineData(null)]
    public void Create_WithNullName_ReturnsNameRequiredError(string? nullName)
    {
        // Act
        var result = StudentName.Create(nullName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.NameRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("  \t  \n  ")]
    public void Create_WithEmptyOrWhitespaceName_ReturnsNameRequiredError(string name)
    {
        // Act
        var result = StudentName.Create(name);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.NameRequired);
    }

    #endregion

    #region Create Method - Invalid Length Tests

    [Fact]
    public void Create_WithNameBelowMinLength_ReturnsInvalidNameError()
    {
        // Arrange
        var shortName = new string('A', StudentName.MinLength - 1);

        // Act
        var result = StudentName.Create(shortName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.InvalidName);
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ReturnsInvalidNameError()
    {
        // Arrange
        var longName = new string('A', StudentName.MaxLength + 1);

        // Act
        var result = StudentName.Create(longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.InvalidName);
    }
    #endregion

    #region Create Method - Invalid Characters Tests

    [Theory]
    [InlineData("Ahmed123")]
    [InlineData("John123Doe")]
    [InlineData("Name@123")]
    [InlineData("Student#1")]
    [InlineData("Test$Name1")]
    public void Create_WithNameContainingNumbers_ReturnsInvalidNameError(string name)
    {
        // Act
        var result = StudentName.Create(name);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.InvalidName);
    }

    [Theory]
    [InlineData("Ahmed@")]
    [InlineData("John#Doe")]
    [InlineData("Mary$Jane")]
    [InlineData("Test!Name")]
    [InlineData("Name&")]
    [InlineData("Student*")]
    [InlineData("Test-Name")]
    [InlineData("Name_")]
    [InlineData("Test.Name")]
    [InlineData("Name,")]
    public void Create_WithNameContainingSpecialCharacters_ReturnsInvalidNameError(string name)
    {
        // Act
        var result = StudentName.Create(name);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.InvalidName);
    }

    [Theory]
    [InlineData("أحمد")]
    [InlineData("محمد علي")]
    [InlineData("فاطمة")]
    [InlineData("عمر")]
    public void Create_WithArabicName_ReturnsSuccess(string arabicName)
    {
        // Act
        var result = StudentName.Create(arabicName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(StudentName.Normalize(arabicName));
    }

    [Theory]
    [InlineData("José")]
    [InlineData("François")]
    [InlineData("Müller")]
    [InlineData("Björk")]
    public void Create_WithAccentedCharacters_ReturnsInvalidNameError(string nameWithAccent)
    {
        // Act
        var result = StudentName.Create(nameWithAccent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.InvalidName);
    }

    #endregion

    #region Normalize Method Tests

    [Theory]
    [InlineData("  Ahmed  ", "Ahmed")]
    [InlineData("\tJohn Doe\t", "John Doe")]
    [InlineData("Mary Jane Watson ", "Mary Jane Watson")]
    [InlineData(" Robert Charles ", "Robert Charles")]
    [InlineData("   Multiple   Spaces   ", "Multiple   Spaces")]
    public void Normalize_WithVariousInputs_TrimsWhitespace(string input, string expected)
    {
        // Act
        var result = StudentName.Normalize(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Ahmed")]
    [InlineData("John Doe")]
    [InlineData("MARY")]
    public void Normalize_PreservesOriginalCase(string input)
    {
        // Act
        var result = StudentName.Normalize(input);

        // Assert
        result.Should().Be(input.Trim());
    }

    [Fact]
    public void Normalize_DoesNotConvertToLowerCase()
    {
        // Arrange
        var input = "JOHN DOE";

        // Act
        var result = StudentName.Normalize(input);

        // Assert
        result.Should().Be("JOHN DOE");
        result.Should().NotBe("john doe");
    }

    #endregion
}
