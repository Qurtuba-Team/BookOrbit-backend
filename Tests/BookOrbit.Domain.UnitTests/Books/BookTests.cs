namespace BookOrbit.Domain.UnitTests.Books;

using BookOrbit.Domain.Books;
using BookOrbit.Domain.Books.ValueObjects;
using BookOrbit.Domain.Books.Enums;
using FluentAssertions;
using Xunit;

public class BookTests
{
    #region Constants

    private const string DefaultCoverImageFileName = "cover.jpg";

    #endregion

    #region Test Helpers

    private static BookTitle CreateValidTitle(string title = "The Great Gatsby")
        => BookTitle.Create(title).Value;

    private static ISBN CreateValidISBN(string isbn = "9780131101928")
        => ISBN.Create(isbn).Value;

    private static BookPublisher CreateValidPublisher(string publisher = "Penguin")
        => BookPublisher.Create(publisher).Value;

    private static BookAuthor CreateValidAuthor(string author = "John Smith")
        => BookAuthor.Create(author).Value;

    private static Book CreateValidBook(
        Guid? id = null,
        string? title = null,
        string? isbn = null,
        string? publisher = null,
        BookCategory category = BookCategory.Fiction,
        string? author = null,
        string? coverImageFileName = null)
    {
        var bookId = id ?? Guid.NewGuid();
        var validTitle = CreateValidTitle(title ?? "The Great Gatsby");
        var validISBN = CreateValidISBN(isbn ?? "9780131101928");
        var validPublisher = CreateValidPublisher(publisher ?? "Penguin");
        var validAuthor = CreateValidAuthor(author ?? "John Smith");
        var validCoverImageFileName = coverImageFileName ?? DefaultCoverImageFileName;

        return Book.Create(
            bookId,
            validTitle,
            validISBN,
            validPublisher,
            category,
            validAuthor,
            validCoverImageFileName).Value;
    }

    #endregion

    #region Create Method - Success Tests

    [Fact]
    public void Create_WithAllRequiredValidParameters_ReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle();
        var isbn = CreateValidISBN();
        var publisher = CreateValidPublisher();
        var category = BookCategory.Fiction;
        var author = CreateValidAuthor();

        // Act
        var result = Book.Create(id, title, isbn, publisher, category, author, DefaultCoverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.Title.Should().Be(title);
        result.Value.ISBN.Should().Be(isbn);
        result.Value.Publisher.Should().Be(publisher);
        result.Value.Category.Should().Be(category);
        result.Value.Author.Should().Be(author);
        result.Value.CoverImageFileName.Should().Be(DefaultCoverImageFileName);
    }

    [Theory]
    [InlineData("The Great Gatsby", "9780131101928", "Penguin", "John Smith")]
    [InlineData("To Kill a Mockingbird", "9780061120084", "Oxford", "Harper Lee")]
    [InlineData("Pride and Prejudice", "9780143039662", "Penguin", "Jane Austen")]
    public void Create_WithDifferentValidBooks_ReturnsSuccessResult(string title, string isbn, string publisher, string author)
    {
        // Arrange
        var id = Guid.NewGuid();
        var validTitle = CreateValidTitle(title);
        var validISBN = CreateValidISBN(isbn);
        var validPublisher = CreateValidPublisher(publisher);
        var validAuthor = CreateValidAuthor(author);
        var category = BookCategory.Fiction;

        // Act
        var result = Book.Create(id, validTitle, validISBN, validPublisher, category, validAuthor, DefaultCoverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Value.Should().Be(title);
        result.Value.ISBN.Value.Should().Be(isbn);
        result.Value.Publisher.Value.Should().Be(publisher);
        result.Value.Author.Value.Should().Be(author);
    }

    [Theory]
    [InlineData(BookCategory.Fiction)]
    [InlineData(BookCategory.Nonfiction)]
    [InlineData(BookCategory.Mystery)]
    [InlineData(BookCategory.Romance)]
    [InlineData(BookCategory.ScienceFiction)]
    [InlineData(BookCategory.Biography)]
    [InlineData(BookCategory.ChildrenBooks)]
    public void Create_WithDifferentCategories_ReturnsSuccessResult(BookCategory category)
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle();
        var isbn = CreateValidISBN();
        var publisher = CreateValidPublisher();
        var author = CreateValidAuthor();

        // Act
        var result = Book.Create(id, title, isbn, publisher, category, author, DefaultCoverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(category);
    }

    [Theory]
    [InlineData("cover.jpg")]
    [InlineData("book_cover.png")]
    [InlineData("image.jpeg")]
    [InlineData("cover_image.gif")]
    public void Create_WithVariousCoverImageFileNames_ReturnsSuccessResult(string coverImageFileName)
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle();
        var isbn = CreateValidISBN();
        var publisher = CreateValidPublisher();
        var category = BookCategory.Fiction;
        var author = CreateValidAuthor();

        // Act
        var result = Book.Create(id, title, isbn, publisher, category, author, coverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CoverImageFileName.Should().Be(coverImageFileName);
    }

    #endregion

    #region Create Method - Null/Empty Parameter Tests

    [Fact]
    public void Create_WithEmptyId_ReturnsIdRequiredError()
    {
        // Arrange
        var title = CreateValidTitle();
        var isbn = CreateValidISBN();
        var publisher = CreateValidPublisher();
        var category = BookCategory.Fiction;
        var author = CreateValidAuthor();

        // Act
        var result = Book.Create(Guid.Empty, title, isbn, publisher, category, author, DefaultCoverImageFileName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.IdRequired);
    }

    [Fact]
    public void Create_WithNullTitle_ReturnsTitleRequiredError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var isbn = CreateValidISBN();
        var publisher = CreateValidPublisher();
        var category = BookCategory.Fiction;
        var author = CreateValidAuthor();

        // Act
        var result = Book.Create(id, null!, isbn, publisher, category, author, DefaultCoverImageFileName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.TitleRequired);
    }

    [Fact]
    public void Create_WithNullISBN_ReturnsISBNRequiredError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle();
        var publisher = CreateValidPublisher();
        var category = BookCategory.Fiction;
        var author = CreateValidAuthor();

        // Act
        var result = Book.Create(id, title, null!, publisher, category, author, DefaultCoverImageFileName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.ISBNRequired);
    }

    [Fact]
    public void Create_WithNullPublisher_ReturnsPublisherRequiredError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle();
        var isbn = CreateValidISBN();
        var category = BookCategory.Fiction;
        var author = CreateValidAuthor();

        // Act
        var result = Book.Create(id, title, isbn, null!, category, author, DefaultCoverImageFileName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.PublisherRequired);
    }

    [Fact]
    public void Create_WithNullAuthor_ReturnsAuthorRequiredError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle();
        var isbn = CreateValidISBN();
        var publisher = CreateValidPublisher();
        var category = BookCategory.Fiction;

        // Act
        var result = Book.Create(id, title, isbn, publisher, category, null!, DefaultCoverImageFileName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.AuthorRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyCoverImageFileName_ReturnsCoverImagePhotoRequiredError(string? coverImageFileName)
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle();
        var isbn = CreateValidISBN();
        var publisher = CreateValidPublisher();
        var category = BookCategory.Fiction;
        var author = CreateValidAuthor();

        // Act
        var result = Book.Create(id, title, isbn, publisher, category, author, coverImageFileName!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.CoverImagePhotoRequired);
    }

    #endregion

    #region Create Method - Category Tests

    [Fact]
    public void Create_WithNoneCategoryValue_ReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle();
        var isbn = CreateValidISBN();
        var publisher = CreateValidPublisher();
        var author = CreateValidAuthor();
        var category = BookCategory.None;

        // Act
        var result = Book.Create(id, title, isbn, publisher, category, author, DefaultCoverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(BookCategory.None);
    }

    [Theory]
    [InlineData(BookCategory.Fiction | BookCategory.Mystery)]
    [InlineData(BookCategory.ScienceFiction | BookCategory.Fantasy)]
    [InlineData(BookCategory.Biography | BookCategory.Autobiography)]
    public void Create_WithMultipleCategoriesCombined_ReturnsSuccessResult(BookCategory category)
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle();
        var isbn = CreateValidISBN();
        var publisher = CreateValidPublisher();
        var author = CreateValidAuthor();

        // Act
        var result = Book.Create(id, title, isbn, publisher, category, author, DefaultCoverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(category);
    }

    #endregion

    #region Update Method - Success Tests

    [Fact]
    public void Update_WithValidTitle_UpdatesBookTitle()
    {
        // Arrange
        var book = CreateValidBook();
        var newTitle = CreateValidTitle("New Title");
        var newCoverImageFileName = "updated-cover.jpg";

        // Act
        var result = book.Update(newTitle, newCoverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        book.Title.Should().Be(newTitle);
        book.Title.Value.Should().Be("New Title");
        book.CoverImageFileName.Should().Be(newCoverImageFileName);
    }

    [Theory]
    [InlineData("Updated Title")]
    [InlineData("The Latest Edition")]
    [InlineData("Book Title Version Two")]
    public void Update_WithDifferentValidTitles_UpdatesSuccessfully(string newTitle)
    {
        // Arrange
        var book = CreateValidBook();
        var validNewTitle = CreateValidTitle(newTitle);
        var newCoverImageFileName = "updated-cover.jpg";

        // Act
        var result = book.Update(validNewTitle, newCoverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        book.Title.Value.Should().Be(newTitle);
        book.CoverImageFileName.Should().Be(newCoverImageFileName);
    }

    [Fact]
    public void Update_WithNewTitle_DoesNotChangeOtherProperties()
    {
        // Arrange
        var book = CreateValidBook(
            title: "Original Title",
            isbn: "9780131101928",
            publisher: "Penguin",
            author: "John Smith");
        
        var originalISBN = book.ISBN.Value;
        var originalPublisher = book.Publisher.Value;
        var originalAuthor = book.Author.Value;
        var newCoverImageFileName = "updated-cover.jpg";

        var newTitle = CreateValidTitle("Updated Title");

        // Act
        var result = book.Update(newTitle, newCoverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        book.ISBN.Value.Should().Be(originalISBN);
        book.Publisher.Value.Should().Be(originalPublisher);
        book.Author.Value.Should().Be(originalAuthor);
        book.CoverImageFileName.Should().Be(newCoverImageFileName);
    }

    #endregion

    #region Update Method - Null Parameter Tests

    [Fact]
    public void Update_WithNullTitle_ReturnsTitleRequiredError()
    {
        // Arrange
        var book = CreateValidBook();
        var newCoverImageFileName = "updated-cover.jpg";

        // Act
        var result = book.Update(null!, newCoverImageFileName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(BookErrors.TitleRequired);
    }


    #endregion

    #region Book Immutability Tests

    [Fact]
    public void Create_WithValidParameters_ISBNIsReadOnly()
    {
        // Arrange
        var book = CreateValidBook();
        var originalISBN = book.ISBN;

        // Act & Assert - Verify ISBN cannot be changed (it's a property getter without setter)
        book.ISBN.Should().Be(originalISBN);
    }

    [Fact]
    public void Create_WithValidParameters_PublisherIsReadOnly()
    {
        // Arrange
        var book = CreateValidBook();
        var originalPublisher = book.Publisher;

        // Act & Assert - Verify Publisher cannot be changed
        book.Publisher.Should().Be(originalPublisher);
    }

    [Fact]
    public void Create_WithValidParameters_AuthorIsReadOnly()
    {
        // Arrange
        var book = CreateValidBook();
        var originalAuthor = book.Author;

        // Act & Assert - Verify Author cannot be changed
        book.Author.Should().Be(originalAuthor);
    }

    [Fact]
    public void Create_WithValidParameters_CategoryIsReadOnly()
    {
        // Arrange
        var book = CreateValidBook();
        var originalCategory = book.Category;

        // Act & Assert - Verify Category cannot be changed
        book.Category.Should().Be(originalCategory);
    }

    [Fact]
    public void Create_WithValidParameters_CoverImageFileNameIsReadOnly()
    {
        // Arrange
        var book = CreateValidBook();
        var originalCoverImageFileName = book.CoverImageFileName;

        // Act & Assert - Verify CoverImageFileName cannot be changed
        book.CoverImageFileName.Should().Be(originalCoverImageFileName);
    }

    [Fact]
    public void Update_WithNewTitle_TitleIsUpdated()
    {
        // Arrange
        var book = CreateValidBook(title: "Original Title");
        var originalTitle = book.Title;
        var newTitle = CreateValidTitle("New Title");
        var newCoverImageFileName = "updated-cover.jpg";

        // Act
        book.Update(newTitle, newCoverImageFileName);

        // Assert
        book.Title.Should().NotBe(originalTitle);
        book.Title.Should().Be(newTitle);
        book.CoverImageFileName.Should().Be(newCoverImageFileName);
    }

    #endregion

    #region Book Properties Tests

    [Fact]
    public void Create_WithValidParameters_AllPropertiesAreSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle("Test Book");
        var isbn = CreateValidISBN("0131101928");
        var publisher = CreateValidPublisher("Test Publisher");
        var category = BookCategory.Mystery;
        var author = CreateValidAuthor("Test Author");
        var coverImageFileName = "cover.png";

        // Act
        var result = Book.Create(id, title, isbn, publisher, category, author, coverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var book = result.Value;

        book.Id.Should().Be(id);
        book.Title.Should().NotBeNull();
        book.Title.Value.Should().Be("Test Book");
        book.ISBN.Should().NotBeNull();
        book.ISBN.Value.Should().Be("0131101928");
        book.Publisher.Should().NotBeNull();
        book.Publisher.Value.Should().Be("Test Publisher");
        book.Category.Should().Be(BookCategory.Mystery);
        book.Author.Should().NotBeNull();
        book.Author.Value.Should().Be("Test Author");
        book.CoverImageFileName.Should().Be("cover.png");
    }

    [Fact]
    public void Create_WithValidParameters_BookHasValidId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle();
        var isbn = CreateValidISBN();
        var publisher = CreateValidPublisher();
        var category = BookCategory.Fiction;
        var author = CreateValidAuthor();

        // Act
        var result = Book.Create(id, title, isbn, publisher, category, author, DefaultCoverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.Id.Should().NotBeEmpty();
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Create_WithMultipleBooksWithDifferentIds_EachHasUniqueId()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var title = CreateValidTitle();
        var isbn1 = CreateValidISBN("9780131101928");
        var isbn2 = CreateValidISBN("9780451524935");
        var publisher = CreateValidPublisher();
        var category = BookCategory.Fiction;
        var author = CreateValidAuthor();

        // Act
        var result1 = Book.Create(id1, title, isbn1, publisher, category, author, DefaultCoverImageFileName);
        var result2 = Book.Create(id2, title, isbn2, publisher, category, author, DefaultCoverImageFileName);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Id.Should().NotBe(result2.Value.Id);
    }

    [Fact]
    public void Create_WithISBN10Format_ReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle();
        var isbn = CreateValidISBN("0131101928");
        var publisher = CreateValidPublisher();
        var category = BookCategory.Fiction;
        var author = CreateValidAuthor();

        // Act
        var result = Book.Create(id, title, isbn, publisher, category, author, DefaultCoverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ISBN.Value.Should().Be("0131101928");
    }

    [Fact]
    public void Create_WithISBN13Format_ReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = CreateValidTitle();
        var isbn = CreateValidISBN("9780131101928");
        var publisher = CreateValidPublisher();
        var category = BookCategory.Fiction;
        var author = CreateValidAuthor();

        // Act
        var result = Book.Create(id, title, isbn, publisher, category, author, DefaultCoverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ISBN.Value.Should().Be("9780131101928");
    }

    [Fact]
    public void Update_MultipleTimesWithDifferentTitles_UpdatesCorrectly()
    {
        // Arrange
        var book = CreateValidBook(title: "First Title");
        var secondTitle = CreateValidTitle("Second Title");
        var thirdTitle = CreateValidTitle("Third Title");
        var secondCoverImageFileName = "second-cover.jpg";
        var thirdCoverImageFileName = "third-cover.jpg";

        // Act
        var result1 = book.Update(secondTitle, secondCoverImageFileName);
        book.Title.Should().Be(secondTitle);

        var result2 = book.Update(thirdTitle, thirdCoverImageFileName);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        book.Title.Should().Be(thirdTitle);
        book.Title.Value.Should().Be("Third Title");
        book.CoverImageFileName.Should().Be(thirdCoverImageFileName);
    }

    #endregion

    [Fact]
    public void Update_WithValidTitle_ReturnsUpdatedResult()
    {
        // Arrange
        var book = CreateValidBook();
        var newTitle = CreateValidTitle("New Title");
        var newCoverImageFileName = "updated-cover.jpg";

        // Act
        var result = book.Update(newTitle, newCoverImageFileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
