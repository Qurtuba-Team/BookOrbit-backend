using BookOrbit.Domain.Common.Entities;
using BookOrbit.Domain.Students.Enums;

namespace BookOrbit.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public AcademicYear? AcademicYear { get; set; }
    public Faculty? Faculty { get; set; }
    public bool HasCompletedOnboarding { get; set; } = false;

    public ICollection<UserInterest> UserInterests { get; set; } = [];
    public ICollection<UserBookInteraction> Interactions { get; set; } = [];
}
