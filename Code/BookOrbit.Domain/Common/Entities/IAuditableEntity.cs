namespace BookOrbit.Domain.Common.Entities;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAtUtc { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset LastModifiedUtc { get; set; }
    string? LastModifiedBy { get; set; }
}
