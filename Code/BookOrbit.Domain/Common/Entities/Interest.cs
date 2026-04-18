namespace BookOrbit.Domain.Common.Entities;

public class Interest
{
    public int Id { get; set; }

    /// <summary>Stores the value of InterestType enum.</summary>
    public int Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<UserInterest> UserInterests { get; set; } = [];
}
