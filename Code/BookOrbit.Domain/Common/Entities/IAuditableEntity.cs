namespace BookOrbit.Domain.Common.Entities;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAtUtc { get;}
    string? CreatedBy { get;}
    DateTimeOffset LastModifiedUtc { get;}
    string? LastModifiedBy { get;}
}