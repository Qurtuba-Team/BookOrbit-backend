using BookOrbit.Application.Common.Enums;
using BookOrbit.Domain.Students.Enums;

namespace BookOrbit.Api.Contracts.Requests.Onboarding;

public record CompleteOnboardingRequest(
    AcademicYear AcademicYear,
    Faculty Faculty,
    List<InterestType> Interests);
