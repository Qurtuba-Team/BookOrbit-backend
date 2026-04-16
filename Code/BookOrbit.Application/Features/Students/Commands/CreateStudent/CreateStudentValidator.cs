namespace BookOrbit.Application.Features.Students.Commands.CreateStudent;
public sealed class CreateStudentValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .StudentNameRules();

        RuleFor(x => x.UniversityMailAddress)
            .Cascade(CascadeMode.Stop)
            .StudentUniversityMailRules();


        RuleFor(x => x.PersonalPhotoFileName)
            .Cascade(CascadeMode.Stop)
            .StudentPersonalImageRules();

        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .StudentPhoneNumberRules()
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));


        RuleFor(x => x.TelegramUserId)
            .Cascade(CascadeMode.Stop)
            .StudentTelegramUserIdRules()
            .When(x => !string.IsNullOrWhiteSpace(x.TelegramUserId));
    }



}

