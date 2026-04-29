namespace BookOrbit.Application.SubcutaneousTests.Students.Queries;

using BookOrbit.Application.Features.Students.Queries.GetStudentById;
using BookOrbit.Application.Features.Students.Queries.GetStudentByUserId;
using BookOrbit.Application.Features.Students.Queries.GetStudentContactInformationByLendingListId;
using BookOrbit.Application.Features.Students.Queries.GetStudentPersonalPhotoFileNameById;
using BookOrbit.Application.Features.Students.Queries.GetStudentPublicProfileById;
using BookOrbit.Application.Features.Students.Queries.GetStudents;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.LendingListings.Enums;
using BookOrbit.Domain.Students.Enums;
using BookOrbit.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class StudentQueriesSubcutaneousTests
{
    [Fact]
    public async Task GetStudentByIdQuery_ShouldReturnStudent()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var student = StudentTestFactory.CreateStudent();
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new GetStudentByIdQueryHandler(
            NullLogger<GetStudentByIdQueryHandler>.Instance,
            context);

        // Act
        var result = await handler.Handle(new GetStudentByIdQuery(student.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(student.Id);
        result.Value.UniversityMailAddress.Should().Be(student.UniversityMail.Value);
    }

    [Fact]
    public async Task GetStudentByUserIdQuery_ShouldReturnStudent()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var student = StudentTestFactory.CreateStudent(userId: "user-42");
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new GetStudentByUserIdQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetStudentByUserIdQuery("user-42"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(student.Id);
    }

    [Fact]
    public async Task GetStudentPublicProfileByIdQuery_ShouldReturnStudentDto()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var student = StudentTestFactory.CreateStudent();
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new GetStudentPublicProfileByIdQueryHandler(
            NullLogger<GetStudentPublicProfileByIdQueryHandler>.Instance,
            context);

        // Act
        var result = await handler.Handle(new GetStudentPublicProfileByIdQuery(student.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(student.Id);
        result.Value.Name.Should().Be(student.Name.Value);
    }

    [Fact]
    public async Task GetStudentPersonalPhotoFileNameByIdQuery_ShouldReturnFileName()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var student = StudentTestFactory.CreateStudent(personalPhotoFileName: "avatar.png");
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var handler = new GetStudentPersonalPhotoFileNameByIdQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetStudentPersonalPhotoFileNameByIdQuery(student.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("avatar.png");
    }

    [Fact]
    public async Task GetStudentsQuery_ShouldReturnPaginatedList()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        context.Students.Add(StudentTestFactory.CreateStudent(name: "Student A", userId: "user-a"));
        context.Students.Add(StudentTestFactory.CreateStudent(name: "Student B", userId: "user-b"));
        await context.SaveChangesAsync();

        var handler = new GetStudentsQueryHandler(context, new StudentQueryService(context));

        var query = new GetStudentsQuery(Page: 1, PageSize: 10, SearchTerm: null, EmailConfirmed: null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeNull();
        result.Value.Items!.Count.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetStudentContactInformationByLendingListIdQuery_ShouldReturnOwnerContactInfo()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;

        var owner = StudentTestFactory.CreateStudent(
            name: "Owner",
            userId: "owner-1",
            phoneNumber: "01011111111",
            telegramUserId: "owner_user");

        var borrower = StudentTestFactory.CreateStudent(name: "Borrower", userId: "borrower-1");
        var book = StudentTestFactory.CreateBook();
        var bookCopy = StudentTestFactory.CreateBookCopy(book, owner.Id, BookCopyCondition.New);
        var lendingRecord = StudentTestFactory.CreateLendingListRecord(bookCopy, now);
        lendingRecord.MarkAsReserved();

        var borrowingRequest = StudentTestFactory.CreateBorrowingRequest(borrower.Id, lendingRecord.Id, now);

        StudentTestFactory.SetNavigation(bookCopy, "Book", book);
        StudentTestFactory.SetNavigation(bookCopy, "Owner", owner);
        StudentTestFactory.SetNavigation(lendingRecord, "BookCopy", bookCopy);
        StudentTestFactory.SetNavigation(borrowingRequest, "BorrowingStudent", borrower);
        StudentTestFactory.SetNavigation(borrowingRequest, "LendingRecord", lendingRecord);

        context.Students.Add(owner);
        context.Students.Add(borrower);
        context.Books.Add(book);
        context.BookCopies.Add(bookCopy);
        context.LendingListRecords.Add(lendingRecord);
        context.BorrowingRequests.Add(borrowingRequest);
        await context.SaveChangesAsync();

        var handler = new GetStudentContactInformationByLendingListIdQueryHandler(
            NullLogger<GetStudentContactInformationByLendingListIdQueryHandler>.Instance,
            context);

        var query = new GetStudentContactInformationByLendingListIdQuery(lendingRecord.Id, borrower.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(owner.Id);
        result.Value.PhoneNumber.Should().Be(owner.PhoneNumber!.Value);
        result.Value.TelegramUserId.Should().Be(owner.TelegramUserId!.Value);
        lendingRecord.State.Should().Be(LendingListRecordState.Reserved);
    }
}
