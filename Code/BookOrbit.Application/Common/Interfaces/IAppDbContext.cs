using BookOrbit.Domain.BorrowingTransactions.BorrowingTransactionEvents;
using BookOrbit.Domain.PointTransactions;

namespace BookOrbit.Application.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<Student> Students { get; }
    public DbSet<Book> Books { get; }
    public DbSet<BookCopy> BookCopies { get; }
    public DbSet<RefreshToken> RefreshTokens { get; }
    public DbSet<LendingListRecord> LendingListRecords { get; }
    public DbSet<BorrowingRequest> BorrowingRequests { get; }
    public DbSet<BorrowingTransaction> BorrowingTransactions { get; }
    public DbSet<BorrowingTransactionEvent> BorrowingTransactionEvents { get; }
    public DbSet<PointTransaction> PointTransactions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

