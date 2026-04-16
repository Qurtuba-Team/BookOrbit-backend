namespace BookOrbit.Domain.Common.Entities;

public abstract class ExpirableEntity : AuditableEntity
{
    protected ExpirableEntity()
    { }
    protected ExpirableEntity(Guid id)
        : base(id)
    {
    }
    public DateTimeOffset? ExpirationDateUtc { get; set; }
}

