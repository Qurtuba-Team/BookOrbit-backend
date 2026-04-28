using BookOrbit.Domain.BookCopies;
using BookOrbit.Domain.BorrowingRequests;
using BookOrbit.Domain.BorrowingTransactions;
using BookOrbit.Domain.BorrowingTransactions.BorrowingReviews;
using BookOrbit.Domain.BorrowingTransactions.BorrowingTransactionEvents;
using BookOrbit.Domain.LendingListings;
using BookOrbit.Domain.PointTransactions;
using MediatR;

namespace BookOrbit.Infrastructure.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator) : IdentityDbContext<AppUser>(options), IAppDbContext
{
    public DbSet<Student> Students => Set<Student>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Book> Books => Set<Book>();

    public DbSet<BookCopy> BookCopies => Set<BookCopy>();

    public DbSet<LendingListRecord> LendingListRecords => Set<LendingListRecord>();

    public DbSet<BorrowingRequest> BorrowingRequests => Set<BorrowingRequest>();

    public DbSet<BorrowingTransaction> BorrowingTransactions => Set<BorrowingTransaction>();

    public DbSet<BorrowingReview> BorrowingReviews => Set<BorrowingReview>();

    public DbSet<BorrowingTransactionEvent> BorrowingTransactionEvents => Set<BorrowingTransactionEvent>();

    public DbSet<PointTransaction> PointTransactions => Set<PointTransaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker.Entries()
            .Where(e => e.Entity is Entity baseEntity && baseEntity.DomainEvents.Count != 0)
            .Select(e => (Entity)e.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }

        foreach (var entity in domainEntities)
        {
            entity.ClearDomainEvents();
        }
    }

}