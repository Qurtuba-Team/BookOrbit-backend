namespace BookOrbit.Domain.Common.Entities;

public class UserInterest
{
    public string UserId { get; set; } = string.Empty;
    public int InterestId { get; set; }

    public Interest Interest { get; set; } = null!;
}
