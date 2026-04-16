using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews.ValueObjects;
using BookOrbit.Domain.PointTransactions.ValueObjects;

namespace BookOrbit.Application.Features.Students;
static public class StudentGeneralValidation
{
    static public IRuleBuilder<T,Guid> StudentIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(StudentErrors.IdRequired.Description)
            .Must(id => id != Guid.Empty).WithMessage(StudentErrors.IdRequired.Description);

    static public IRuleBuilder<T, string> StudentNameRules<T>(this IRuleBuilder<T, string> ruleBuilder) =>
         ruleBuilder
            .NotEmpty().WithMessage(StudentErrors.NameRequired.Description)
            .MaximumLength(StudentName.MaxLength).WithMessage(StudentErrors.InvalidName.Description)
            .MinimumLength(StudentName.MinLength).WithMessage(StudentErrors.InvalidName.Description);

    static public IRuleBuilder<T, string> StudentUniversityMailRules<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(StudentErrors.UniversityMailRequired.Description)
            .MaximumLength(UniversityMail.MaxLength).WithMessage(StudentErrors.InvalidUniversityMail.Description)
            .EmailAddress().WithMessage(StudentErrors.InvalidUniversityMail.Description)
            .Must(x => x.EndsWith(@"std.mans.edu.eg")).WithMessage(StudentErrors.InvalidUniversityMail.Description);//Simple Email check , the rest in domain

    static public IRuleBuilder<T, string> StudentPersonalImageRules<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(StudentErrors.PersonalImageRequired.Description);

    static public IRuleBuilderOptions<T, string?> StudentPhoneNumberRules<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder
        .MinimumLength(PhoneNumber.MinLength).WithMessage(PhoneNumberErrors.InvalidPhoneNumber.Description)
        .MaximumLength(PhoneNumber.MaxLength).WithMessage(PhoneNumberErrors.InvalidPhoneNumber.Description)
        .Must(PhoneNumberCorrectPrefix).WithMessage(PhoneNumberErrors.InvalidPhoneNumber.Description)
        .Matches(@"^\d+$"); // Only digits allowed

    static public IRuleBuilderOptions<T, string?> StudentTelegramUserIdRules<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder
            .MinimumLength(TelegramUserId.MinLength).WithMessage(TelegramUserIdErrors.InvalidTelegramUserId.Description)
            .MaximumLength(TelegramUserId.MaxLength).WithMessage(TelegramUserIdErrors.InvalidTelegramUserId.Description)
            .Matches("^[a-zA-Z0-9_]+$").WithMessage(TelegramUserIdErrors.InvalidTelegramUserId.Description); // lower chars, digits, underscores only 


    private static bool PhoneNumberCorrectPrefix(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return true; // optional

        return PhoneNumber.Prefixes
            .Any(phoneNumber.StartsWith);
    }
}