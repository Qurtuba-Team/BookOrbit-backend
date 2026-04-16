namespace BookOrbit.Domain.UnitTests.Students.ValueObjects;

using BookOrbit.Domain.Students.ValueObjects;
using BookOrbit.Domain.Students;

public class UniversityMailTests
{
    #region Constants

    private const string ValidDomain = "@std.mans.edu.eg";
    private const string NormalizedEmail = "student.name@std.mans.edu.eg";
    #endregion

    #region Create Method - Success Tests

    [Theory]
    [InlineData("student.name@std.mans.edu.eg")]
    [InlineData("student123@std.mans.edu.eg")]
    [InlineData("test.user@std.mans.edu.eg")]
    [InlineData("a.b@std.mans.edu.eg")]
    public void Create_WithValidEmail_ReturnsSuccessResult(string validEmail)
    {
        // Act
        var result = UniversityMail.Create(validEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(UniversityMail.Normalize(validEmail));
    }

    [Theory]
    [InlineData("  student.name@std.mans.edu.eg  ")]
    [InlineData("\tstudent.name@std.mans.edu.eg\t")]
    [InlineData("student.name@std.mans.edu.eg ")]
    [InlineData(" student.name@std.mans.edu.eg")]
    public void Create_WithEmailAndWhitespace_NormalizesAndReturnsSuccess(string emailWithWhitespace)
    {
        // Act
        var result = UniversityMail.Create(emailWithWhitespace);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(UniversityMail.Normalize(emailWithWhitespace));
    }

    [Theory]
    [InlineData("STUDENT.NAME@STD.MANS.EDU.EG")]
    [InlineData("StUdEnT.NaMe@STD.MANS.EDU.EG")]
    [InlineData("Student.Name@Std.Mans.Edu.Eg")]
    public void Create_WithCaseVariations_NormalizesAndReturnsSuccess(string email)
    {
        // Act
        var result = UniversityMail.Create(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(UniversityMail.Normalize(email));
    }

    [Theory]
    [InlineData("student.name+tag@std.mans.edu.eg")]
    [InlineData("student_name@std.mans.edu.eg")]
    [InlineData("student-name@std.mans.edu.eg")]
    [InlineData("student%name@std.mans.edu.eg")]
    [InlineData("student.name123@std.mans.edu.eg")]
    public void Create_WithSpecialCharactersAllowed_ReturnsSuccess(string email)
    {
        // Act
        var result = UniversityMail.Create(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmailExactlyAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        var emailAtMaxLength = new string('a', UniversityMail.MaxLength - ValidDomain.Length) + ValidDomain;

        // Act
        var result = UniversityMail.Create(emailAtMaxLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Create Method - Null/Empty Tests

    [Theory]
    [InlineData(null)]
    public void Create_WithNullEmail_ReturnsUniversityMailRequiredError(string? nullEmail)
    {
        // Act
        var result = UniversityMail.Create(nullEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.UniversityMailRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("  \t  \n  ")]
    public void Create_WithEmptyOrWhitespaceEmail_ReturnsUniversityMailRequiredError(string email)
    {
        // Act
        var result = UniversityMail.Create(email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.UniversityMailRequired);
    }

    #endregion

    #region Create Method - Invalid Format Tests

    [Theory]
    [InlineData("student.namestd.mans.edu.eg")]
    [InlineData("studentnameatstdmansedueg")]
    [InlineData("student@")]
    [InlineData("@std.mans.edu.eg")]
    [InlineData("student@@std.mans.edu.eg")]
    [InlineData("student@std.mans")]
    [InlineData("student@std")]
    public void Create_WithMissingOrInvalidAtSymbol_ReturnsInvalidUniversityMailError(string email)
    {
        // Act
        var result = UniversityMail.Create(email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.InvalidUniversityMail);
    }

    [Theory]
    [InlineData("student.name@gmail.com")]
    [InlineData("student.name@yahoo.com")]
    [InlineData("student.name@hotmail.com")]
    [InlineData("student.name@std.mans.com")]
    [InlineData("student.name@std.edu.eg")]
    [InlineData("student.name@mans.edu.eg")]
    [InlineData("student.name@std.mans.org")]
    [InlineData("student.name@example.std.mans.edu.eg")]
    public void Create_WithWrongDomain_ReturnsInvalidUniversityMailError(string email)
    {
        // Act
        var result = UniversityMail.Create(email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.InvalidUniversityMail);
    }

    [Theory]
    [InlineData("student name@std.mans.edu.eg")]
    [InlineData("student\tname@std.mans.edu.eg")]
    [InlineData("student\nname@std.mans.edu.eg")]
    public void Create_WithSpacesInLocalPart_ReturnsInvalidUniversityMailError(string email)
    {
        // Act
        var result = UniversityMail.Create(email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.InvalidUniversityMail);
    }

    [Fact]
    public void Create_WithEmailExceedingMaxLength_ReturnsInvalidUniversityMailError()
    {
        // Arrange
        var longEmail = new string('a', UniversityMail.MaxLength - ValidDomain.Length +1) + ValidDomain;

        // Act
        var result = UniversityMail.Create(longEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.InvalidUniversityMail);
    }

    #endregion

    #region Normalize Method Tests

    [Theory]
    [InlineData("STUDENT.NAME@STD.MANS.EDU.EG", NormalizedEmail)]
    [InlineData("  student.name@std.mans.edu.eg  ", NormalizedEmail)]
    [InlineData("StUdEnT.NaMe@STD.MANS.EDU.EG", NormalizedEmail)]
    [InlineData("\tstudent.name@std.mans.edu.eg\t", NormalizedEmail)]
    [InlineData("STUDENT.NAME@STD.MANS.EDU.EG ", NormalizedEmail)]
    public void Normalize_WithVariousInputs_ReturnsNormalizedEmail(string input, string expected)
    {
        // Act
        var result = UniversityMail.Normalize(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
