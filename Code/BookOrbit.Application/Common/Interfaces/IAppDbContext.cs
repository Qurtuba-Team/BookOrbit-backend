
using BookOrbit.Domain.Otps;

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
    public DbSet<BorrowingReview> BorrowingReviews { get; }
    public DbSet<BorrowingTransactionEvent> BorrowingTransactionEvents { get; }
    public DbSet<PointTransaction> PointTransactions { get; }
    public DbSet<Otp> Otps { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

