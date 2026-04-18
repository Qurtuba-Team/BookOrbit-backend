using BookOrbit.Domain.BookCopies;
using BookOrbit.Domain.Common.Entities;
using BookOrbit.Domain.LendingListings;

namespace BookOrbit.Infrastructure.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options), IAppDbContext
{
    public DbSet<Student> Students => Set<Student>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Book> Books => Set<Book>();

    public DbSet<BookCopy> BookCopies => Set<BookCopy>();

    public DbSet<LendingListRecord> LendingListRecords => Set<LendingListRecord>();

    public DbSet<Interest> Interests => Set<Interest>();

    public DbSet<UserInterest> UserInterests => Set<UserInterest>();

    public DbSet<UserBookInteraction> UserBookInteractions => Set<UserBookInteraction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}