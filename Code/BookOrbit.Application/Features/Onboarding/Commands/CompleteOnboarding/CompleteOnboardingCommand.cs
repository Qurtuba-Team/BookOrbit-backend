namespace BookOrbit.Application.Features.Onboarding.Commands.CompleteOnboarding;

public record CompleteOnboardingCommand(
    string UserId,
    AcademicYear AcademicYear,
    Faculty Faculty,
    List<InterestType> Interests) : IRequest<Result<bool>>;
