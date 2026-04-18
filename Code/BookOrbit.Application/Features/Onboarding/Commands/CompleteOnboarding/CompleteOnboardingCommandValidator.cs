namespace BookOrbit.Application.Features.Onboarding.Commands.CompleteOnboarding;

public class CompleteOnboardingCommandValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingCommandValidator()
    {
        RuleFor(x => x.AcademicYear)
            .IsInEnum()
            .WithMessage("AcademicYear must be a valid enum value.");

        RuleFor(x => x.Faculty)
            .IsInEnum()
            .WithMessage("Faculty must be a valid enum value.");

        RuleFor(x => x.Interests)
            .NotEmpty()
            .WithMessage("At least one interest must be selected.")
            .Must(list => list is { Count: >= 1 and <= 5 })
            .WithMessage("You must select between 1 and 5 interests.");

        RuleForEach(x => x.Interests)
            .IsInEnum()
            .WithMessage("Each interest must be a valid InterestType value.");
    }
}
