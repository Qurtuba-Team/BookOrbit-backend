namespace BookOrbit.Application.Features.Onboarding.Commands.CompleteOnboarding;

public class CompleteOnboardingCommandHandler(
    IOnboardingRepository onboardingRepository,
    HybridCache cache,
    ILogger<CompleteOnboardingCommandHandler> logger) : IRequestHandler<CompleteOnboardingCommand, Result<bool>>
{
    private const string RecommendationsCacheKeyPrefix = "recommendations:";

    public async Task<Result<bool>> Handle(CompleteOnboardingCommand command, CancellationToken ct)
    {
        var userExists = await onboardingRepository.UserExistsAsync(command.UserId, ct);

        if (!userExists)
        {
            logger.LogWarning("Onboarding failed — user not found: {UserId}", command.UserId);
            return OnboardingErrors.UserNotFound;
        }

        await onboardingRepository.CompleteOnboardingAsync(
            command.UserId,
            command.AcademicYear,
            command.Faculty,
            command.Interests,
            ct);

        var cacheKey = $"{RecommendationsCacheKeyPrefix}{command.UserId}";
        await cache.RemoveAsync(cacheKey, ct);

        logger.LogInformation(
            "Onboarding completed for UserId={UserId}. Cache invalidated.",
            command.UserId);

        return true;
    }
}
