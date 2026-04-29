namespace BookOrbit.Application.UnitTests.Mappers;

using BookOrbit.Application.Features.BorrowingRequests.Dtos;
using BookOrbit.Domain.BookCopies;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.Books;
using BookOrbit.Domain.Books.Enums;
using BookOrbit.Domain.Books.ValueObjects;
using BookOrbit.Domain.BorrowingRequests;
using BookOrbit.Domain.BorrowingRequests.Enums;
using BookOrbit.Domain.Common.ValueObjects;
using BookOrbit.Domain.LendingListings;
using BookOrbit.Domain.PointTransactions.ValueObjects;
using BookOrbit.Domain.Students;
using BookOrbit.Domain.Students.ValueObjects;
using BookOrbit.Domain.UnitTests.Helpers;
using FluentAssertions;
using System.Reflection;
using Xunit;

public class BorrowingRequestMapperTests
{
    [Fact]
    public void FromEntity_ShouldMapScalarFields()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var expirationDate = now.AddDays(10);
        var id = Guid.NewGuid();
        var borrowingStudentId = Guid.NewGuid();
        var lendingRecordId = Guid.NewGuid();

        var borrowingRequest = BorrowingRequest.Create(
            id,
            borrowingStudentId,
            lendingRecordId,
            expirationDate,
            now).Value;

        // Act
        var dto = BorrowingRequestDto.FromEntity(borrowingRequest);

        // Assert
        dto.Id.Should().Be(id);
        dto.BorrowingStudentId.Should().Be(borrowingStudentId);
        dto.LendingRecordId.Should().Be(lendingRecordId);
        dto.State.Should().Be(BorrowingRequestState.Pending);
        dto.ExpirationDateUtc.Should().Be(expirationDate);
    }

    [Fact]
    public void Projection_ShouldMapNestedFields()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var createdAt = now.AddMinutes(-5);
        var expirationDate = now.AddDays(10);

        var studentName = StudentName.Create("Test Student").Value;
        var universityMail = UniversityMail.Create("student@std.mans.edu.eg").Value;
        var phoneNumber = PhoneNumber.Create("01012345678").Value;

        var student = Student.Create(
            Guid.NewGuid(),
            studentName,
            universityMail,
            "photo.png",
            "user-1",
            phoneNumber).Value;

        var bookTitle = BookTitle.Create("Test Book").Value;
        var isbn = ISBN.Create("9780306406157").Value;
        var publisher = BookPublisher.Create("Test Publisher").Value;
        var author = BookAuthor.Create("Test Author").Value;
        var book = Book.Create(
            Guid.NewGuid(),
            bookTitle,
            isbn,
            publisher,
            BookCategory.Science,
            author,
            "cover.png").Value;

        var bookCopy = BookCopy.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            book.Id,
            BookCopyCondition.New).Value;

        SetNavigation(bookCopy, "Book", book);

        var cost = Point.Create(5).Value;
        var lendingRecord = LendingListRecord.Create(
            Guid.NewGuid(),
            bookCopy.Id,
            LendingListRecord.MinBorrowingDurationInDays,
            cost,
            expirationDate,
            now).Value;

        SetNavigation(lendingRecord, "BookCopy", bookCopy);

        var borrowingRequest = BorrowingRequest.Create(
            Guid.NewGuid(),
            student.Id,
            lendingRecord.Id,
            expirationDate,
            now).Value;

        borrowingRequest.SetCreatedAt(createdAt);
        SetNavigation(borrowingRequest, "BorrowingStudent", student);
        SetNavigation(borrowingRequest, "LendingRecord", lendingRecord);

        // Act
        var dto = BorrowingRequestListItemDto.Projection.Compile()(borrowingRequest);

        // Assert
        dto.Id.Should().Be(borrowingRequest.Id);
        dto.BorrowingStudentId.Should().Be(student.Id);
        dto.LendingRecordId.Should().Be(lendingRecord.Id);
        dto.LendingStudentId.Should().Be(bookCopy.OwnerId);
        dto.BorrowingStudentName.Should().Be(student.Name.Value);
        dto.BookTitle.Should().Be(book.Title.Value);
        dto.BookId.Should().Be(book.Id);
        dto.BookCopyId.Should().Be(bookCopy.Id);
        dto.State.Should().Be(borrowingRequest.State);
        dto.ExpirationDateUtc.Should().Be(borrowingRequest.ExpirationDateUtc);
        dto.CreatedAtUtc.Should().Be(createdAt);
    }

    private static void SetNavigation<TTarget, TValue>(TTarget target, string propertyName, TValue value)
        where TTarget : class
    {
        var property = typeof(TTarget).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property != null && property.CanWrite)
        {
            property.SetValue(target, value);
            return;
        }

        var field = typeof(TTarget).GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }
}
