namespace BookOrbit.Domain.Common.Entities;
public abstract class ConcurrencyEntity : AuditableEntity
{
    public byte[] RowVersion { get; set; } = null!;
    protected ConcurrencyEntity()
    {
    }

    protected ConcurrencyEntity(Guid id) : base(id)
    {
    }
}