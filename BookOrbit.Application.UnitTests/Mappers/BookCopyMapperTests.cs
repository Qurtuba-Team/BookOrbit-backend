namespace BookOrbit.Application.UnitTests.Mappers;

using BookOrbit.Application.Features.BookCopies.Dtos;
using BookOrbit.Application.Features.Books.Dtos;
using BookOrbit.Domain.BookCopies;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.Books;
using BookOrbit.Domain.Books.Enums;
using BookOrbit.Domain.Books.ValueObjects;
using FluentAssertions;
using Xunit;

public class BookCopyMapperTests
{
    [Fact]
    public void FromEntity_ShouldMapBookCopyWithBookDetails()
    {
        // Arrange
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

        var bookCoverImageUrl = "https://cdn.example.com/cover.png";
        var bookDto = BookDto.FromEntity(book, bookCoverImageUrl);

        var bookCopy = BookCopy.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            book.Id,
            BookCopyCondition.New).Value;

        var ownerName = "Owner Name";
        var isListed = true;

        // Act
        var dto = BookCopyDtoWithBookDetails.FromEntity(bookCopy, ownerName, bookDto, isListed);

        // Assert
        dto.Id.Should().Be(bookCopy.Id);
        dto.BookId.Should().Be(bookCopy.BookId);
        dto.OwnerId.Should().Be(bookCopy.OwnerId);
        dto.Condition.Should().Be(bookCopy.Condition);
        dto.State.Should().Be(bookCopy.State);
        dto.OwnerName.Should().Be(ownerName);
        dto.Title.Should().Be(book.Title.Value);
        dto.ISBN.Should().Be(book.ISBN.Value);
        dto.Publisher.Should().Be(book.Publisher.Value);
        dto.Category.Should().Be(book.Category);
        dto.Author.Should().Be(book.Author.Value);
        dto.IsListed.Should().Be(isListed);
        dto.BookCoverImageUrl.Should().Be(bookCoverImageUrl);
    }
}
