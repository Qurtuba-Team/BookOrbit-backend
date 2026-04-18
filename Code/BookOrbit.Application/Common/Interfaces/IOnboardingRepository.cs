namespace BookOrbit.Application.Common.Interfaces;

/// <summary>
/// Abstraction for onboarding persistence — avoids a circular reference between
/// Application and Infrastructure by hiding AppUser behind domain-level DTOs.
/// </summary>
public interface IOnboardingRepository
{
    /// <summary>
    /// Returns true if the user with the given id exists.
    /// </summary>
    Task<bool> UserExistsAsync(string userId, CancellationToken ct);

    /// <summary>
    /// Persists the onboarding data (academic year, faculty, interests) for the user
    /// and marks HasCompletedOnboarding = true.
    /// </summary>
    Task CompleteOnboardingAsync(
        string userId,
        AcademicYear academicYear,
        Faculty faculty,
        List<InterestType> interestTypes,
        CancellationToken ct);
}
