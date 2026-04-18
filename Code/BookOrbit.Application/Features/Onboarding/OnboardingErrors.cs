namespace BookOrbit.Application.Features.Onboarding;

public static class OnboardingErrors
{
    public static readonly Error UserNotFound = Error.NotFound(
        "Onboarding.UserNotFound",
        "The user was not found.");

    public static readonly Error AlreadyCompleted = Error.Conflict(
        "Onboarding.AlreadyCompleted",
        "Onboarding has already been completed for this user.");
}
