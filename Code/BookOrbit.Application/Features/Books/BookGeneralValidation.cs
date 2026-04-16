
namespace BookOrbit.Application.Features.Books;
static public class BookGeneralValidation
{
    static public IRuleBuilder<T, Guid> BookIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
    ruleBuilder
        .NotEmpty().WithMessage(BookErrors.IdRequired.Description)
        .Must(id => id != Guid.Empty).WithMessage(BookErrors.IdRequired.Description);

    static public IRuleBuilder<T, string> BookTitleRules<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder.
        NotEmpty().WithMessage(BookErrors.TitleRequired.Description)
        .MinimumLength(BookTitle.MinLength).WithMessage(BookErrors.InvalidTitle.Description)
        .MaximumLength(BookTitle.MaxLength).WithMessage(BookErrors.InvalidTitle.Description);

    static public IRuleBuilder<T, string> BookISBNRules<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder.
        NotEmpty().WithMessage(BookErrors.ISBNRequired.Description)
        .MinimumLength(ISBN.MinLength).WithMessage(BookErrors.InvalidISBN.Description)
        .MaximumLength(ISBN.MaxLength).WithMessage(BookErrors.InvalidISBN.Description);

    static public IRuleBuilder<T, string> BookPublisherRules<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder.
        NotEmpty().WithMessage(BookErrors.PublisherRequired.Description)
        .MinimumLength(BookPublisher.MinLength).WithMessage(BookErrors.InvalidPublisher.Description)
        .MaximumLength(BookPublisher.MaxLength).WithMessage(BookErrors.InvalidPublisher.Description);

    static public IRuleBuilder<T, BookCategory> BookCategoryRules<T>(this IRuleBuilder<T, BookCategory> ruleBuilder) =>
      ruleBuilder.
      IsInEnum().WithMessage(BookErrors.InvalidCategory.Description);

    static public IRuleBuilder<T, string> BookAuthorRules<T>(this IRuleBuilder<T, string> ruleBuilder) =>
      ruleBuilder.
      NotEmpty().WithMessage(BookErrors.AuthorRequired.Description)
      .MinimumLength(BookAuthor.MinLength).WithMessage(BookErrors.InvalidAuthor.Description)
      .MaximumLength(BookAuthor.MaxLength).WithMessage(BookErrors.InvalidAuthor.Description);

    static public IRuleBuilder<T, string> BookCoverImageFileNameRules<T>(this IRuleBuilder<T, string> ruleBuilder) =>
    ruleBuilder.
    NotEmpty().WithMessage(BookErrors.CoverImagePhotoRequired.Description);

}