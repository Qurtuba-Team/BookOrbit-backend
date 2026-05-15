
namespace BookOrbit.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor
    (TimeProvider timeProvider,
    ICurrentUser user) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var utcNow = timeProvider.GetUtcNow();
        var userId = string.IsNullOrWhiteSpace(user?.Id) ? "system" : user.Id;

        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.Entity is IAuditableEntity))
        {
            if ((entry.State is EntityState.Added or EntityState.Modified) || entry.HasChangedOwnedEntities())
            {
                var auditableEntity = (IAuditableEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entry.Property(nameof(IAuditableEntity.CreatedBy)).CurrentValue = userId;
                    entry.Property(nameof(IAuditableEntity.CreatedAtUtc)).CurrentValue = utcNow;
                }

                entry.Property(nameof(IAuditableEntity.LastModifiedBy)).CurrentValue = userId;
                entry.Property(nameof(IAuditableEntity.LastModifiedUtc)).CurrentValue = utcNow;

                foreach (var ownedEntry in entry.References)
                {
                    if (ownedEntry.TargetEntry is { Entity: IAuditableEntity ownedEntity } && ownedEntry.TargetEntry.State is EntityState.Added or EntityState.Modified)
                    {
                        if (ownedEntry.TargetEntry.State == EntityState.Added)
                        {
                            ownedEntry.TargetEntry.Property(nameof(IAuditableEntity.CreatedBy)).CurrentValue = userId;
                            ownedEntry.TargetEntry.Property(nameof(IAuditableEntity.CreatedAtUtc)).CurrentValue = utcNow;
                        }

                        ownedEntry.TargetEntry.Property(nameof(IAuditableEntity.LastModifiedBy)).CurrentValue = userId;
                        ownedEntry.TargetEntry.Property(nameof(IAuditableEntity.LastModifiedUtc)).CurrentValue = utcNow;
                    }
                }
            }
        }
    }
}

public static class AuditableEntityInterceptorExtensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
    entry.References.Any(r =>
        r.TargetEntry?.Metadata.IsOwned() == true &&
        (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));

}
