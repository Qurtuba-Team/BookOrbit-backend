namespace BookOrbit.Application.SubcutaneousTests.Students.Commands;

using BookOrbit.Application.Features.Students.Commands.CreateStudent;
using BookOrbit.Application.Features.Students.Commands.StateMachien.ActivateStudent;
using BookOrbit.Application.Features.Students.Commands.StateMachien.ApproveStudent;
using BookOrbit.Application.Features.Students.Commands.StateMachien.BanStudent;
using BookOrbit.Application.Features.Students.Commands.StateMachien.PendStudent;
using BookOrbit.Application.Features.Students.Commands.StateMachien.RejectStudent;
using BookOrbit.Application.Features.Students.Commands.StateMachien.UnBanStudent;
using BookOrbit.Application.Features.Students.Commands.UpdateStudent;
using BookOrbit.Domain.Common.Results;
using BookOrbit.Application.SubcutaneousTests.Students.TestDoubles;
using BookOrbit.Domain.Students.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using BookOrbit.Infrastructure.Services.ConcurrencyServices;
using BookOrbit.Infrastructure.Common.Errors;
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
        var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
        var student = StudentTestFactory.CreateStudent();
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new UpdateStudentCommandHandler(
            NullLogger<UpdateStudentCommandHandler>.Instance,
            context,
            cache,
            concurrencyService);

        var command = new UpdateStudentCommand(
            student.Id,
            "Updated Name",
            "updated.png",
            StudentTestFactory.CreateRowVersionBase64());

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
        var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
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
            concurrencyService,
            NullLogger<ApproveStudentCommandHandler>.Instance,
            cache);

        var command = new ApproveStudentCommand(student.Id, StudentTestFactory.CreateRowVersionBase64());

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
        var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
        var student = StudentTestFactory.CreateStudent();
        StudentTestFactory.SetCreatedAt(student, DateTimeOffset.UtcNow.AddMinutes(-10));
        student.MarkAsApproved(DateTimeOffset.UtcNow);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new ActivateStudentCommandHandler(
            context,
            concurrencyService,
            NullLogger<ActivateStudentCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new ActivateStudentCommand(student.Id, StudentTestFactory.CreateRowVersionBase64()), CancellationToken.None);

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
        var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
        var student = StudentTestFactory.CreateStudent();
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new BanStudentCommandHandler(
            context,
            concurrencyService,
            NullLogger<BanStudentCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new BanStudentCommand(student.Id, StudentTestFactory.CreateRowVersionBase64()), CancellationToken.None);

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
        var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
        var student = StudentTestFactory.CreateStudent();
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new RejectStudentCommandHandler(
            context,
            concurrencyService,
            NullLogger<RejectStudentCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new RejectStudentCommand(student.Id, StudentTestFactory.CreateRowVersionBase64()), CancellationToken.None);

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
        var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
        var student = StudentTestFactory.CreateStudent();
        student.MarkAsRejected();
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new PendStudentCommandHandler(
            context,
            concurrencyService,
            NullLogger<PendStudentCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new PendStudentCommand(student.Id, StudentTestFactory.CreateRowVersionBase64()), CancellationToken.None);

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
        var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
        var student = StudentTestFactory.CreateStudent();
        student.MarkAsBanned();
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new UnBanStudentCommandHandler(
            context,
            concurrencyService,
            NullLogger<UnBanStudentCommandHandler>.Instance,
            cache);

        // Act
        var result = await handler.Handle(new UnBanStudentCommand(student.Id, StudentTestFactory.CreateRowVersionBase64()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.State.Should().Be(StudentState.UnBanned);
    }

    [Theory]
    [MemberData(nameof(ConcurrencyTokenRequiredScenarios))]
    public async Task Commands_ShouldReturnConcurrencyTokenRequired_WhenRowVersionIsMissing(
        string scenario,
        Func<Task<Result<Updated>>> act)
    {
        var result = await act();

        result.IsFailure.Should().BeTrue($"{scenario} should fail when the concurrency token is missing");
        result.Errors.Should().Contain(e => e.Code == InfrastrucureConcurrencyErrors.ConcurrencyTokenRequired.Code);
    }

    [Theory]
    [MemberData(nameof(InvalidConcurrencyFormatScenarios))]
    public async Task Commands_ShouldReturnInvalidConcurrencyFormat_WhenRowVersionIsMalformed(
        string scenario,
        Func<Task<Result<Updated>>> act)
    {
        var result = await act();

        result.IsFailure.Should().BeTrue($"{scenario} should fail when the concurrency token is malformed");
        result.Errors.Should().Contain(e => e.Code == InfrastrucureConcurrencyErrors.InvalidConcurrencyFormat.Code);
    }

    public static IEnumerable<object[]> ConcurrencyTokenRequiredScenarios()
    {
        yield return new object[] { nameof(ActivateStudentCommandHandler), CreateActivateStudentExecution(null) };
        yield return new object[] { nameof(ApproveStudentCommandHandler), CreateApproveStudentExecution(null) };
        yield return new object[] { nameof(BanStudentCommandHandler), CreateBanStudentExecution(null) };
        yield return new object[] { nameof(PendStudentCommandHandler), CreatePendStudentExecution(null) };
        yield return new object[] { nameof(RejectStudentCommandHandler), CreateRejectStudentExecution(null) };
        yield return new object[] { nameof(UnBanStudentCommandHandler), CreateUnBanStudentExecution(null) };
        yield return new object[] { nameof(UpdateStudentCommandHandler), CreateUpdateStudentExecution(null) };
    }

    public static IEnumerable<object[]> InvalidConcurrencyFormatScenarios()
    {
        const string invalidRowVersion = "not-a-base64-token";

        yield return new object[] { nameof(ActivateStudentCommandHandler), CreateActivateStudentExecution(invalidRowVersion) };
        yield return new object[] { nameof(ApproveStudentCommandHandler), CreateApproveStudentExecution(invalidRowVersion) };
        yield return new object[] { nameof(BanStudentCommandHandler), CreateBanStudentExecution(invalidRowVersion) };
        yield return new object[] { nameof(PendStudentCommandHandler), CreatePendStudentExecution(invalidRowVersion) };
        yield return new object[] { nameof(RejectStudentCommandHandler), CreateRejectStudentExecution(invalidRowVersion) };
        yield return new object[] { nameof(UnBanStudentCommandHandler), CreateUnBanStudentExecution(invalidRowVersion) };
        yield return new object[] { nameof(UpdateStudentCommandHandler), CreateUpdateStudentExecution(invalidRowVersion) };
    }

    private static Func<Task<Result<Updated>>> CreateActivateStudentExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
            var student = StudentTestFactory.CreateStudent();
            StudentTestFactory.SetCreatedAt(student, DateTimeOffset.UtcNow.AddMinutes(-10));
            student.MarkAsApproved(DateTimeOffset.UtcNow);
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var handler = new ActivateStudentCommandHandler(
                context,
                concurrencyService,
                NullLogger<ActivateStudentCommandHandler>.Instance,
                cache);

            return await handler.Handle(new ActivateStudentCommand(student.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreateApproveStudentExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
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
                concurrencyService,
                NullLogger<ApproveStudentCommandHandler>.Instance,
                cache);

            return await handler.Handle(new ApproveStudentCommand(student.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreateBanStudentExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
            var student = StudentTestFactory.CreateStudent();
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var handler = new BanStudentCommandHandler(
                context,
                concurrencyService,
                NullLogger<BanStudentCommandHandler>.Instance,
                cache);

            return await handler.Handle(new BanStudentCommand(student.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreatePendStudentExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
            var student = StudentTestFactory.CreateStudent();
            student.MarkAsRejected();
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var handler = new PendStudentCommandHandler(
                context,
                concurrencyService,
                NullLogger<PendStudentCommandHandler>.Instance,
                cache);

            return await handler.Handle(new PendStudentCommand(student.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreateRejectStudentExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
            var student = StudentTestFactory.CreateStudent();
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var handler = new RejectStudentCommandHandler(
                context,
                concurrencyService,
                NullLogger<RejectStudentCommandHandler>.Instance,
                cache);

            return await handler.Handle(new RejectStudentCommand(student.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreateUnBanStudentExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
            var student = StudentTestFactory.CreateStudent();
            student.MarkAsBanned();
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var handler = new UnBanStudentCommandHandler(
                context,
                concurrencyService,
                NullLogger<UnBanStudentCommandHandler>.Instance,
                cache);

            return await handler.Handle(new UnBanStudentCommand(student.Id, rowVersion!), CancellationToken.None);
        };

    private static Func<Task<Result<Updated>>> CreateUpdateStudentExecution(string? rowVersion)
        => async () =>
        {
            using var context = StudentTestFactory.CreateDbContext();
            var cache = StudentTestFactory.CreateHybridCache();
            var concurrencyService = new ConcurrencyService(context, NullLogger<ConcurrencyService>.Instance);
            var student = StudentTestFactory.CreateStudent();
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var handler = new UpdateStudentCommandHandler(
                NullLogger<UpdateStudentCommandHandler>.Instance,
                context,
                cache,
                concurrencyService);

            return await handler.Handle(
                new UpdateStudentCommand(student.Id, "Updated Name", "updated.png", rowVersion!),
                CancellationToken.None);
        };
}
