namespace BookOrbit.Domain.Common.Entities;

public interface IExpirableEntity
{
    DateTimeOffset? ExpirationDateUtc { get; set; }
}

