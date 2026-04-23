namespace BookOrbit.Domain.Books;

static public class BookErrors
{
    private const string ClassName = nameof(Book);
    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName,"Id","Id");
    static public readonly Error TitleRequired = DomainCommonErrors.RequiredProp(ClassName,"Title","Title");
    static public readonly Error InvalidTitle = DomainCommonErrors.InvalidProp(ClassName, "Title", "Title", $"It must be between {BookTitle.MinLength} and {BookTitle.MaxLength} [Arabic/English] characters");
    static public readonly Error PublisherRequired = DomainCommonErrors.RequiredProp(ClassName,"Publisher","Publisher");
    static public readonly Error CoverImagePhotoRequired = DomainCommonErrors.RequiredProp(ClassName,"CoverImagePhoto", "Cover Image Photo");
    static public readonly Error InvalidPublisher = DomainCommonErrors.InvalidProp(ClassName, "Publisher", "Publisher", $"It must be between {BookAuthor.MinLength}  and  {BookAuthor.MaxLength} [Arabic/English] characters");
    static public readonly Error AuthorRequired = DomainCommonErrors.RequiredProp(ClassName,"Author","Author");
    static public readonly Error InvalidAuthor = DomainCommonErrors.InvalidProp(ClassName, "Author", "Author", $"It must be between {BookAuthor.MinLength} and {BookAuthor.MaxLength} [Arabic/Englilsh] characters");
    static public readonly Error InvalidCategory = DomainCommonErrors.InvalidProp(ClassName, "BookCategory", "Book Category", $"Invalid category value");
    static public readonly Error CategoryRequired = DomainCommonErrors.RequiredProp(ClassName, "BookCategory", "Book Category");
    static public readonly Error BookAlreadyAvailable = DomainCommonErrors.InvalidStateTransition(ClassName,"Available", "Available");

    #region ISBN
    static public readonly Error ISBNRequired = DomainCommonErrors.RequiredProp(ClassName,"ISBN","ISBN");
    static public readonly Error InvalidISBN = DomainCommonErrors.InvalidProp(ClassName, "ISBN", "ISBN", "It must be a valid ISBN-10 or ISBN-13 format");
    #endregion
}

