namespace BookOrbit.Application.SubcutaneousTests.Students.Commands;

using BookOrbit.Application.Features.Students.Commands.CreateStudent;
using BookOrbit.Application.Features.Students.Commands.StateMachien.ActivateStudent;
using BookOrbit.Application.Features.Students.Commands.StateMachien.ApproveStudent;
using BookOrbit.Application.Features.Students.Commands.StateMachien.BanStudent;
using BookOrbit.Application.Features.Students.Commands.StateMachien.PendStudent;
using BookOrbit.Application.Features.Students.Commands.StateMachien.RejectStudent;
using BookOrbit.Application.Features.Students.Commands.StateMachien.UnBanStudent;
using BookOrbit.Application.Features.Students.Commands.UpdateStudent;
using BookOrbit.Application.SubcutaneousTests.Students.TestDoubles;
using BookOrbit.Domain.Students.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class StudentCommandsSubcutaneousTests
{
    [Fact]
    public async Task CreateStudentCommand_ShouldPersistStudent()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var identityService = new FakeIdentityService();
        var handler = new CreateStudentCommandHandler(
            NullLogger<CreateStudentCommandHandler>.Instance,
            context,
            cache,
            new FakeMaskingService(),
            identityService);

        var command = new CreateStudentCommand(
            Name: "Test Student",
            UniversityMailAddress: "student@std.mans.edu.eg",
            PersonalPhotoFileName: "C:/images/photo.png",
            Password: "Pass123",
            PhoneNumber: "01012345678");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UniversityMailAddress.Should().Be("student@std.mans.edu.eg");
        result.Value.PhoneNumber.Should().Be("201012345678");
        context.Students.Should().HaveCount(1);
        context.Students.Single().PersonalPhotoFileName.Should().Be("photo.png");
    }

    [Fact]
    public async Task UpdateStudentCommand_ShouldUpdateStudent()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var student = StudentTestFactory.CreateStudent();
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new UpdateStudentCommandHandler(
            NullLogger<UpdateStudentCommandHandler>.Instance,
            context,
            cache);

        var command = new UpdateStudentCommand(student.Id, "Updated Name", "updated.png");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.Name.Value.Should().Be("Updated Name");
        student.PersonalPhotoFileName.Should().Be("updated.png");
    }

    [Fact]
    public async Task ApproveStudentCommand_ShouldApproveStudent_WhenEmailConfirmed()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var identityService = new FakeIdentityService();
        var student = StudentTestFactory.CreateStudent();
        StudentTestFactory.SetCreatedAt(student, DateTimeOffset.UtcNow.AddMinutes(-10));
        identityService.SetEmailConfirmed(student.UserId, true);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new ApproveStudentCommandHandler(
            context,
            TimeProvider.System,
            identityService,
            new FakeMaskingService(),
            NullLogger<ApproveStudentCommandHandler>.Instance,
            cache);

        var command = new ApproveStudentCommand(student.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Approved);
        student.JoinDateUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task ActivateStudentCommand_ShouldActivateApprovedStudent()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var student = StudentTestFactory.CreateStudent();
        StudentTestFactory.SetCreatedAt(student, DateTimeOffset.UtcNow.AddMinutes(-10));
        student.MarkAsApproved(DateTimeOffset.UtcNow);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new ActivateStudentCommandHandler(
            context,
            NullLogger<ActivateStudentCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new ActivateStudentCommand(student.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Active);
    }

    [Fact]
    public async Task BanStudentCommand_ShouldBanStudent()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var student = StudentTestFactory.CreateStudent();
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new BanStudentCommandHandler(
            context,
            NullLogger<BanStudentCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new BanStudentCommand(student.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Banned);
    }

    [Fact]
    public async Task RejectStudentCommand_ShouldRejectStudent()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var student = StudentTestFactory.CreateStudent();
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new RejectStudentCommandHandler(
            context,
            NullLogger<RejectStudentCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new RejectStudentCommand(student.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Rejected);
    }

    [Fact]
    public async Task PendStudentCommand_ShouldPendRejectedStudent()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var student = StudentTestFactory.CreateStudent();
        student.MarkAsRejected();
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new PendStudentCommandHandler(
            NullLogger<PendStudentCommandHandler>.Instance,
            context,
            cache);

        // Act
        var result = await handler.Handle(new PendStudentCommand(student.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.Pending);
    }

    [Fact]
    public async Task UnBanStudentCommand_ShouldUnBanStudent()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var cache = StudentTestFactory.CreateHybridCache();
        var student = StudentTestFactory.CreateStudent();
        student.MarkAsBanned();
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new UnBanStudentCommandHandler(
            context,
            NullLogger<UnBanStudentCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new UnBanStudentCommand(student.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.UnBanned);
    }
}
