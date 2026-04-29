
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

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if ((entry.State is EntityState.Added or EntityState.Modified) || entry.HasChangedOwnedEntities())
            {

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.CreatedAtUtc = utcNow;
                }

                entry.Entity.LastModifiedBy = userId;
                entry.Entity.LastModifiedUtc = utcNow;

                foreach (var ownedEntry in entry.References)
                {
                    if (ownedEntry.TargetEntry is { Entity: AuditableEntity ownedEntity } && ownedEntry.TargetEntry.State is EntityState.Added or EntityState.Modified)
                    {
                        if (ownedEntry.TargetEntry.State == EntityState.Added)
                        {
                            ownedEntity.CreatedBy = userId;
                            ownedEntity.CreatedAtUtc = utcNow;
                        }

                        ownedEntity.LastModifiedBy = userId;
                        ownedEntity.LastModifiedUtc = utcNow;
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
