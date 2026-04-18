namespace BookOrbit.Application.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<Student> Students { get; }
    public DbSet<Book> Books { get; }
    public DbSet<BookCopy> BookCopies { get; }
    public DbSet<RefreshToken> RefreshTokens { get; }
    public DbSet<LendingListRecord> LendingListRecords { get; }
    public DbSet<Interest> Interests { get; }
    public DbSet<UserInterest> UserInterests { get; }
    public DbSet<UserBookInteraction> UserBookInteractions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

