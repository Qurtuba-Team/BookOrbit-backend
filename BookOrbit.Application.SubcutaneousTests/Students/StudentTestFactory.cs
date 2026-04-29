namespace BookOrbit.Application.SubcutaneousTests.Students;

using BookOrbit.Domain.BookCopies;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.Books;
using BookOrbit.Domain.Books.Enums;
using BookOrbit.Domain.Books.ValueObjects;
using BookOrbit.Domain.BorrowingRequests;
using BookOrbit.Domain.BorrowingTransactions;
using BookOrbit.Domain.Common.ValueObjects;
using BookOrbit.Domain.LendingListings;
using BookOrbit.Domain.PointTransactions.ValueObjects;
using BookOrbit.Domain.Students;
using BookOrbit.Domain.Students.ValueObjects;
using BookOrbit.Infrastructure.Data;
using System.Runtime.CompilerServices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

internal static class StudentTestFactory
{
    public static AppDbContext CreateDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, new NoOpMediator());
    }

    public static HybridCache CreateHybridCache()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();
        services.AddHybridCache();

        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }

    public static Student CreateStudent(
        string name = "Test Student",
        string email = "student@std.mans.edu.eg",
        string userId = "user-1",
        string personalPhotoFileName = "photo.png",
        string phoneNumber = "01012345678",
        string? telegramUserId = null)
    {
        var studentName = StudentName.Create(name).Value;
        var universityMail = UniversityMail.Create(email).Value;
        var phone = PhoneNumber.Create(phoneNumber).Value;
        TelegramUserId? telegram = null;

        if (!string.IsNullOrWhiteSpace(telegramUserId))
        {
            telegram = TelegramUserId.Create(telegramUserId).Value;
        }

        return Student.Create(
            Guid.NewGuid(),
            studentName,
            universityMail,
            personalPhotoFileName,
            userId,
            phone,
            telegram).Value;
    }

    public static Book CreateBook(
        string title = "Test Book",
        string isbnValue = "9780306406157",
        string publisherValue = "Test Publisher",
        string authorValue = "Test Author")
    {
        var titleObject = BookTitle.Create(title).Value;
        var isbn = ISBN.Create(isbnValue).Value;
        var publisher = BookPublisher.Create(publisherValue).Value;
        var author = BookAuthor.Create(authorValue).Value;

        return Book.Create(
            Guid.NewGuid(),
            titleObject,
            isbn,
            publisher,
            BookCategory.Science,
            author,
            "cover.png").Value;
    }

    public static BookCopy CreateBookCopy(Book book, Guid ownerId, BookCopyCondition condition = BookCopyCondition.New)
    {
        return BookCopy.Create(Guid.NewGuid(), ownerId, book.Id, condition).Value;
    }

    public static LendingListRecord CreateLendingListRecord(BookCopy bookCopy, DateTimeOffset now)
    {
        var expirationDate = now.AddDays(10);
        var cost = Point.Create(5).Value;

        return LendingListRecord.Create(
            Guid.NewGuid(),
            bookCopy.Id,
            LendingListRecord.MinBorrowingDurationInDays,
            cost,
            expirationDate,
            now).Value;
    }

    public static BorrowingRequest CreateBorrowingRequest(
        Guid borrowingStudentId,
        Guid lendingRecordId,
        DateTimeOffset now)
    {
        var expirationDate = now.AddDays(5);

        return BorrowingRequest.Create(
            Guid.NewGuid(),
            borrowingStudentId,
            lendingRecordId,
            expirationDate,
            now).Value;
    }

    public static BorrowingTransaction CreateBorrowingTransaction(
        Guid borrowingRequestId,
        Guid lenderStudentId,
        Guid borrowerStudentId,
        Guid bookCopyId,
        DateTimeOffset now,
        int borrowingDurationInDays = LendingListRecord.MinBorrowingDurationInDays)
    {
        return BorrowingTransaction.Create(
            Guid.NewGuid(),
            borrowingRequestId,
            lenderStudentId,
            borrowerStudentId,
            bookCopyId,
            now.AddDays(borrowingDurationInDays),
            now).Value;
    }

    public static T SetCreatedAt<T>(T entity, DateTimeOffset createdAtUtc) where T : class
    {
        var property = typeof(T).GetProperty("CreatedAtUtc", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (property != null && property.CanWrite)
        {
            property.SetValue(entity, createdAtUtc);
        }
        else
        {
            var field = typeof(T).GetField("<CreatedAtUtc>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(entity, createdAtUtc);
        }

        return entity;
    }

    public static void SetNavigation<TTarget, TValue>(TTarget target, string propertyName, TValue value)
        where TTarget : class
    {
        var property = typeof(TTarget).GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        if (property != null && property.CanWrite)
        {
            property.SetValue(target, value);
            return;
        }

        var field = typeof(TTarget).GetField($"<{propertyName}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }

    private sealed class NoOpMediator : IMediator
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => Task.FromResult(default(TResponse)!);

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => Task.FromResult<object?>(null);

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => Task.CompletedTask;

        public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield break;
        }

        public async IAsyncEnumerable<object?> CreateStream(
            object request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield break;
        }
    }
}
