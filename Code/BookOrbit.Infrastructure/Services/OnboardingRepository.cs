using BookOrbit.Application.Common.Enums;
using BookOrbit.Domain.Common.Entities;
using BookOrbit.Domain.Students.Enums;

namespace BookOrbit.Infrastructure.Services;

public class OnboardingRepository(
    UserManager<AppUser> userManager,
    AppDbContext context,
    ILogger<OnboardingRepository> logger) : IOnboardingRepository
{
    public async Task<bool> UserExistsAsync(string userId, CancellationToken ct)
    {
        return await userManager.FindByIdAsync(userId) is not null;
    }

    public async Task CompleteOnboardingAsync(
        string userId,
        AcademicYear academicYear,
        Faculty faculty,
        List<InterestType> interestTypes,
        CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            logger.LogWarning("CompleteOnboardingAsync: user {UserId} not found.", userId);
            return;
        }

        user.AcademicYear = academicYear;
        user.Faculty = faculty;
        user.HasCompletedOnboarding = true;

        await userManager.UpdateAsync(user);

        // Remove existing interests then re-add
        var existingInterests = context.UserInterests.Where(ui => ui.UserId == userId);
        context.UserInterests.RemoveRange(existingInterests);

        // Resolve InterestType values to Interest rows by Type int value
        var interestTypeInts = interestTypes.Select(it => (int)it).ToList();
        var interestIds = await context.Interests
            .Where(i => interestTypeInts.Contains(i.Type))
            .Select(i => new { i.Id })
            .ToListAsync(ct);

        var userInterests = interestIds.Select(i => new UserInterest
        {
            UserId = userId,
            InterestId = i.Id
        });

        await context.UserInterests.AddRangeAsync(userInterests, ct);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Onboarding persisted for UserId={UserId}, Faculty={Faculty}, AcademicYear={Year}, Interests={Count}",
            userId, faculty, academicYear, interestTypes.Count);
    }
}
