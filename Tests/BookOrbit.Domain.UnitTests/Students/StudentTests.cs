namespace BookOrbit.Domain.UnitTests.Students;

using BookOrbit.Domain.Students.ValueObjects;
using BookOrbit.Domain.Students;
using BookOrbit.Domain.Common.ValueObjects;
using BookOrbit.Domain.PointTransactions.ValueObjects;
using BookOrbit.Domain.Students.Enums;
using BookOrbit.Domain.UnitTests.Helpers;

public class StudentTests
{
    #region Constants

    private const string DefaultPhotoFileName = "photo.jpg";
    private const string DefaultUserId = "user123";
    private const string AlternativePhotoFileName = "new_photo.jpg";

    #endregion

    #region Test Helpers

    private static StudentName CreateValidName(string name = "John Doe")
        => StudentName.Create(name).Value;

    private static UniversityMail CreateValidEmail(string email = "student@std.mans.edu.eg")
        => UniversityMail.Create(email).Value;

    private static PhoneNumber CreateValidPhoneNumber(string phone = "201012345678")
        => PhoneNumber.Create(phone).Value;

    private static TelegramUserId CreateValidTelegramId(string telegramId = "johndoe123")
        => TelegramUserId.Create(telegramId).Value;

    private static Student CreateValidStudent(
        string? name = null,
        string? email = null,
        string? phoneNumber = null,
        string? telegramId = null,
        string? photoFileName = null,
        string? userId = null)
    {
        var id = Guid.NewGuid();
        var validName = CreateValidName(name ?? "John Doe");
        var validEmail = CreateValidEmail(email ?? "student@std.mans.edu.eg");
        var validPhotoFileName = photoFileName ?? DefaultPhotoFileName;
        var validUserId = userId ?? DefaultUserId;

        PhoneNumber? validPhoneNumber = null;
        if (phoneNumber != null)
        {
            validPhoneNumber = CreateValidPhoneNumber(phoneNumber);
        }

        TelegramUserId? validTelegramId = null;
        if (telegramId != null)
        {
            validTelegramId = CreateValidTelegramId(telegramId);
        }

        // If both are null, provide phone number as default
        if (validPhoneNumber == null && validTelegramId == null)
        {
            validPhoneNumber = CreateValidPhoneNumber();
        }

        return Student.Create(id, validName, validEmail, validPhotoFileName, validUserId, validPhoneNumber, validTelegramId).Value;
    }

    #endregion

    #region Create Method - Success Tests

    [Fact]
    public void Create_WithAllRequiredValidParameters_ReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = CreateValidName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Student.Create(id, name, email, DefaultPhotoFileName, DefaultUserId, phoneNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.Name.Should().Be(name);
        result.Value.UniversityMail.Should().Be(email);
        result.Value.PersonalPhotoFileName.Should().Be(DefaultPhotoFileName);
        result.Value.UserId.Should().Be(DefaultUserId);
        result.Value.PhoneNumber.Should().Be(phoneNumber);
        result.Value.TelegramUserId.Should().BeNull();
        result.Value.State.Should().Be(StudentState.Pending);
        result.Value.Points.Value.Should().Be(1);
        result.Value.JoinDateUtc.Should().BeNull();
    }

    [Fact]
    public void Create_WithPhoneNumberAndTelegramId_ReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = CreateValidName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();
        var telegramId = CreateValidTelegramId();

        // Act
        var result = Student.Create(id, name, email, DefaultPhotoFileName, DefaultUserId, phoneNumber, telegramId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PhoneNumber.Should().Be(phoneNumber);
        result.Value.TelegramUserId.Should().Be(telegramId);
    }

    [Fact]
    public void Create_WithOnlyTelegramId_ReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = CreateValidName();
        var email = CreateValidEmail();
        var telegramId = CreateValidTelegramId();

        // Act
        var result = Student.Create(id, name, email, DefaultPhotoFileName, DefaultUserId, null, telegramId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PhoneNumber.Should().BeNull();
        result.Value.TelegramUserId.Should().Be(telegramId);
    }

    [Fact]
    public void Create_WithPhoneNumber_ReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = CreateValidName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Student.Create(id, name, email, DefaultPhotoFileName, DefaultUserId, phoneNumber,null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TelegramUserId.Should().BeNull();
        result.Value.PhoneNumber.Should().Be(phoneNumber);
    }

    #endregion

    #region Create Method - Null Parameter Tests

    [Fact]
    public void Create_WithEmptyId_ReturnsIdRequiredError()
    {
        // Arrange
        var name = CreateValidName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Student.Create(Guid.Empty, name, email, DefaultPhotoFileName, DefaultUserId, phoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.IdRequired);
    }

    [Fact]
    public void Create_WithNullName_ReturnsNameRequiredError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Student.Create(id, null!, email, DefaultPhotoFileName, DefaultUserId, phoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.NameRequired);
    }

    [Fact]
    public void Create_WithNullEmail_ReturnsUniversityMailRequiredError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = CreateValidName();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Student.Create(id, name, null!, DefaultPhotoFileName, DefaultUserId, phoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.UniversityMailRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyUserId_ReturnsUserIdRequiredError(string? userId)
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = CreateValidName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Student.Create(id, name, email, DefaultPhotoFileName, userId!, phoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.UserIdRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyFileName_ReturnsPersonalImageRequiredError(string? fileName)
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = CreateValidName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Student.Create(id, name, email, fileName!, DefaultUserId, phoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.PersonalImageRequired);
    }

    [Fact]
    public void Create_WithBothCommunicationMethodsNull_ReturnsAtLeastOneCommunicationMethodError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = CreateValidName();
        var email = CreateValidEmail();

        // Act
        var result = Student.Create(id, name, email, DefaultPhotoFileName, DefaultUserId, null, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.AtLeastOneCommunicationMethod);
    }

    #endregion

    #region Update Method Tests

    [Fact]
    public void Update_WithValidParameters_UpdatesNameAndPhoto()
    {
        // Arrange
        var student = CreateValidStudent(name: "John Doe");
        var newName = CreateValidName("Jane Doe");

        // Act
        var result = student.Update(newName, AlternativePhotoFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.Name.Should().Be(newName);
        student.PersonalPhotoFileName.Should().Be(AlternativePhotoFileName);
    }

    [Fact]
    public void Update_WhenStudentIsBanned_ReturnsCannotUpdateError()
    {
        // Arrange
        var student = CreateValidStudent();
        student.MarkAsBanned();

        var newName = CreateValidName("New Name");

        // Act
        var result = student.Update(newName, AlternativePhotoFileName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.CannotUpdateABannedStudent);
    }

    [Fact]
    public void Update_WithNullName_ReturnsNameRequiredError()
    {
        // Arrange
        var student = CreateValidStudent();

        // Act
        var result = student.Update(null!, AlternativePhotoFileName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.NameRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyPhotoFileName_ReturnsPersonalImageRequiredError(string fileName)
    {
        // Arrange
        var student = CreateValidStudent();
        var newName = CreateValidName();

        // Act
        var result = student.Update(newName, fileName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.PersonalImageRequired);
    }

    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("student_photo.png")]
    [InlineData("image.gif")]
    [InlineData("file.jpeg")]
    public void Update_WithVariousFileNames_ReturnsSuccess(string fileName)
    {
        // Arrange
        var student = CreateValidStudent();
        var newName = CreateValidName();

        // Act
        var result = student.Update(newName, fileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.PersonalPhotoFileName.Should().Be(fileName);
    }

    #endregion

    #region Points Tests

    [Fact]
    public void AddPoints_WithValidPoints_IncreasesPoints()
    {
        // Arrange
        var student = CreateValidStudent();
        var pointsToAdd = new Point(2);
        var originalPoints = student.Points.Value;

        // Act
        var result = student.AddPoints(pointsToAdd);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.Points.Value.Should().Be(originalPoints + pointsToAdd.Value);
    }

    [Fact]
    public void DeductPoints_WithSufficientPoints_DecreasesPoints()
    {
        // Arrange
        var student = CreateValidStudent();
        student.AddPoints(new Point(2));
        var pointsToDeduct = new Point(1);
        var originalPoints = student.Points.Value;

        // Act
        var result = student.DeductPoints(pointsToDeduct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.Points.Value.Should().Be(originalPoints - pointsToDeduct.Value);
    }

    [Fact]
    public void DeductPoints_WithInsufficientPoints_ReturnsInsufficientPointsError()
    {
        // Arrange
        var student = CreateValidStudent();
        var pointsToDeduct = new Point(student.Points.Value + 1);

        // Act
        var result = student.DeductPoints(pointsToDeduct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.InsufficientPoints);
        student.Points.Value.Should().Be(1);
    }

    #endregion

    #region Approve Method Tests

    [Fact]
    public void Approve_WithValidJoinDate_TransitionsToApprovedState()
    {
        // Arrange
        var student = CreateValidStudent();

        student.SetCreatedAt(DateTimeOffset.UtcNow);

        var joinDate = student.CreatedAtUtc.AddDays(1);

        // Act
        var result = student.MarkAsApproved(joinDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Approved);
        student.JoinDateUtc.Should().Be(joinDate);
    }

    [Fact]
    public void Approve_WithJoinDateBeforeCreation_ReturnsInvalidJoinDateError()
    {
        // Arrange
        var student = CreateValidStudent();

        student.SetCreatedAt(DateTimeOffset.UtcNow);

        var invalidJoinDate = student.CreatedAtUtc.AddDays(-1);

        // Act
        var result = student.MarkAsApproved(invalidJoinDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(StudentErrors.InvalidJoinDate);
    }

    [Fact]
    public void Approve_WithJoinDateEqualToCreationDate_ReturnsSuccess()
    {
        // Arrange
        var student = CreateValidStudent();
        student.SetCreatedAt(DateTimeOffset.UtcNow);
        var joinDate = student.CreatedAtUtc;

        // Act
        var result = student.MarkAsApproved(joinDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Approved);
    }


    #endregion

    #region State Transition - Pending State Tests

    [Fact]
    public void Pending_Student_CanTransitionToApproved()
    {
        // Arrange
        var student = CreateValidStudent();
        student.SetCreatedAt(DateTimeOffset.UtcNow);

        // Act
        var result = student.MarkAsApproved(student.CreatedAtUtc.AddDays(1));

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Approved);
    }


    [Fact]
    public void Pending_Student_CanTransitionToRejected()
    {
        // Arrange
        var student = CreateValidStudent();

        // Act
        var result = student.MarkAsRejected();

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Rejected);
    }

    [Fact]
    public void Pending_Student_CanTransitionToBanned()
    {
        // Arrange
        var student = CreateValidStudent();

        // Act
        var result = student.MarkAsBanned();

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Banned);
    }

    [Fact]
    public void Pending_Student_CannotTransitionToActive()
    {
        // Arrange
        var student = CreateValidStudent();

        // Act
        var result = student.MarkAsActivated();

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region State Transition - Approved State Tests

    [Fact]
    public void Approved_Student_CanTransitionToActive()
    {
        // Arrange
        var student = CreateValidStudent();
        student.SetCreatedAt(DateTimeOffset.UtcNow);
        student.MarkAsApproved(student.CreatedAtUtc.AddDays(1));

        // Act
        var result = student.MarkAsActivated();

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Active);
    }


    [Fact]
    public void Approved_Student_CanTransitionToBanned()
    {
        // Arrange
        var student = CreateValidStudent();
        student.SetCreatedAt(DateTimeOffset.UtcNow);
        student.MarkAsApproved(student.CreatedAtUtc.AddDays(1));

        // Act
        var result = student.MarkAsBanned();

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Banned);
    }


    [Fact]
    public void Approved_Student_CannotTransitionToRejected()
    {
        // Arrange
        var student = CreateValidStudent();
        student.SetCreatedAt(DateTimeOffset.UtcNow);
        student.MarkAsApproved(student.CreatedAtUtc.AddDays(1));

        // Act
        var result = student.MarkAsRejected();

        // Assert
        result.IsFailure.Should().BeTrue();
    }


    #endregion

    #region State Transition - Active State Tests

    [Fact]
    public void Active_Student_CanTransitionToBanned()
    {
        // Arrange
        var student = CreateValidStudent();
        student.SetCreatedAt(DateTimeOffset.UtcNow);
        student.MarkAsApproved(student.CreatedAtUtc.AddDays(1));
        student.MarkAsActivated();

        // Act
        var result = student.MarkAsBanned();

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Banned);
    }


    [Fact]
    public void Active_Student_CannotTransitionToApproved()
    {
        // Arrange
        var student = CreateValidStudent();
        student.SetCreatedAt(DateTimeOffset.UtcNow);
        student.MarkAsApproved(student.CreatedAtUtc.AddDays(1));
        student.MarkAsActivated();

        // Act
        var result = student.MarkAsApproved(student.CreatedAtUtc.AddDays(2));

        // Assert
        result.IsFailure.Should().BeTrue();
    }


    [Fact]
    public void Active_Student_CannotTransitionToRejected()
    {
        // Arrange
        var student = CreateValidStudent();
        student.SetCreatedAt(DateTimeOffset.UtcNow);
        student.MarkAsApproved(student.CreatedAtUtc.AddDays(1));
        student.MarkAsActivated();

        // Act
        var result = student.MarkAsRejected();

        // Assert
        result.IsFailure.Should().BeTrue();
    }


    #endregion

    #region State Transition - Banned State Tests

    [Fact]
    public void Banned_Student_CanTransitionToUnBanned()
    {
        // Arrange
        var student = CreateValidStudent();
        student.MarkAsBanned();

        // Act
        var result = student.MarkAsUnBanned();

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.UnBanned);
    }

    [Fact]
    public void Banned_Student_CannotTransitionToApproved()
    {
        // Arrange
        var student = CreateValidStudent();
        student.MarkAsBanned();

        // Act
        var result = student.MarkAsApproved(DateTimeOffset.UtcNow.AddDays(1));

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region State Transition - Rejected State Tests

    [Fact]
    public void Rejected_Student_CanTransitionToPending()
    {
        // Arrange
        var student = CreateValidStudent();
        student.MarkAsRejected();

        // Act
        var result = student.MarkAsPend();

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Pending);
    }

    [Fact]
    public void Rejected_Student_CanTransitionToBanned()
    {
        // Arrange
        var student = CreateValidStudent();
        student.MarkAsRejected();

        // Act
        var result = student.MarkAsBanned();

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Banned);
    }

    [Fact]
    public void Rejected_Student_CannotTransitionToApproved()
    {
        // Arrange
        var student = CreateValidStudent();
        student.MarkAsRejected();

        // Act
        var result = student.MarkAsApproved(DateTimeOffset.UtcNow.AddDays(1));

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region State Transition - UnBanned State Tests

    [Fact]
    public void UnBanned_Student_CanTransitionToActive()
    {
        // Arrange
        var student = CreateValidStudent();
        student.MarkAsBanned();
        student.MarkAsUnBanned();

        // Act
        var result = student.MarkAsActivated();

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Active);
    }

    [Fact]
    public void UnBanned_Student_CanTransitionToBanned()
    {
        // Arrange
        var student = CreateValidStudent();
        student.MarkAsBanned();
        student.MarkAsUnBanned();

        // Act
        var result = student.MarkAsBanned();

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Banned);
    }

    [Fact]
    public void UnBanned_Student_CannotTransitionToPending()
    {
        // Arrange
        var student = CreateValidStudent();
        student.MarkAsBanned();
        student.MarkAsUnBanned();

        // Act
        var result = student.MarkAsPend();

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Complete State Flow Tests

    [Fact]
    public void CompleteFlow_Pending_Approved_Active_Banned_UnBanned_Active()
    {
        // Arrange
        var student = CreateValidStudent();
        student.State.Should().Be(StudentState.Pending);

        // Act & Assert - Approve
        var approveResult = student.MarkAsApproved(DateTimeOffset.UtcNow.AddDays(1));
        approveResult.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Approved);

        // Act & Assert - Activate
        var activateResult = student.MarkAsActivated();
        activateResult.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Active);

        // Act & Assert - Ban
        var banResult = student.MarkAsBanned();
        banResult.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Banned);

        // Act & Assert - UnBan
        var unbanResult = student.MarkAsUnBanned();
        unbanResult.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.UnBanned);

        // Act & Assert - Activate again
        var reactivateResult = student.MarkAsActivated();
        reactivateResult.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Active);
    }

    [Fact]
    public void CompleteFlow_Pending_Rejected_Pending_Banned()
    {
        // Arrange
        var student = CreateValidStudent();

        // Act & Assert - Reject
        var rejectResult = student.MarkAsRejected();
        rejectResult.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Rejected);

        // Act & Assert - Pend
        var pendResult = student.MarkAsPend();
        pendResult.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Pending);

        // Act & Assert - Ban
        var banResult = student.MarkAsBanned();
        banResult.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Banned);
    }

    #endregion
}