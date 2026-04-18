using BookOrbit.Application.Features.Onboarding.Commands.CompleteOnboarding;
using BookOrbit.Api.Contracts.Requests.Onboarding;

namespace BookOrbit.Api.Common.Mappers;

public static class OnboardingMapper
{
    public static CompleteOnboardingCommand ToCommand(
        this CompleteOnboardingRequest request,
        string userId)
    {
        return new CompleteOnboardingCommand(
            UserId: userId,
            AcademicYear: request.AcademicYear,
            Faculty: request.Faculty,
            Interests: request.Interests);
    }
}
