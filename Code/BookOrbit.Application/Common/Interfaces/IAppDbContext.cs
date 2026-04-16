namespace BookOrbit.Application.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<Student> Students { get; }
    public DbSet<Book> Books { get; }
    public DbSet<BookCopy> BookCopies { get; }
    public DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

