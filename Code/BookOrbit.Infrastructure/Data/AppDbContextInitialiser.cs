
namespace BookOrbit.Infrastructure.Data;


public class AppDbContextInitialiser(
    ILogger<AppDbContextInitialiser> logger,
    UserManager<AppUser> userManager,
    AppDbContext context,
    RoleManager<IdentityRole> roleManager)
{
    private const int DatabaseStartupMaxAttempts = 12;
    private static readonly TimeSpan DatabaseStartupDelay = TimeSpan.FromSeconds(5);

    public async Task InitialiseAsync()
    {
        for (var attempt = 1; attempt <= DatabaseStartupMaxAttempts; attempt++)
        {
            try
            {
                if (await DatabaseAlreadyExistsAsync())
                {
                    logger.LogInformation("Database already exists. Skipping database creation.");
                    return;
                }

                await context.Database.MigrateAsync();
                return;
            }
            catch (Exception ex) when (IsTransientDatabaseStartupFailure(ex) && attempt < DatabaseStartupMaxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "Database is not ready yet. Retrying database initialization attempt {Attempt} of {MaxAttempts} in {DelaySeconds} seconds.",
                    attempt,
                    DatabaseStartupMaxAttempts,
                    DatabaseStartupDelay.TotalSeconds);

                await Task.Delay(DatabaseStartupDelay);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        #region Roles

        var studentRoleName = nameof(IdentityRoles.student);
        var adminRoleName = nameof(IdentityRoles.admin);

        if (!await roleManager.RoleExistsAsync(studentRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole(studentRoleName));
            logger.LogInformation("Student role created");
        }

        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
            logger.LogInformation("Admin role created");
        }

        #endregion

        #region Students

        var students = new List<AppUser>
        {
            new()
            {
                Email = "student1@std.mans.edu.eg",
                UserName = "first student",
                EmailConfirmed = true
            },
            new()
            {
                Email = "student2@std.mans.edu.eg",
                UserName = "الطالب الثاني",
                EmailConfirmed = true
            },
            new()
            {
                Email = "student3@std.mans.edu.eg",
                UserName = "الطالب الثالث",
                EmailConfirmed = true
            },
            new()
            {
                Email = "student4@std.mans.edu.eg",
                UserName = "الطالب الرابع",
                EmailConfirmed = true
            },
            new()
            {
                Email = "student5@std.mans.edu.eg",
                UserName = "الطالب الخامس",
                EmailConfirmed = true
            }
        };

        string studentPassword = "sa123456";

        int index = 1;

        foreach (var studentUser in students)
        {
            var user = await CreateUserIfNotExistsAsync(studentUser, studentPassword, studentRoleName);

            if (user is null)
                continue;


            await CreateStudentIfNotExistsAsync(user, index);
            index++;
        }

        #endregion

        #region Admin

        var admin = new AppUser
        {
            Email = "admin@bookorbit.com",
            UserName = "the admin",
            EmailConfirmed = true
        };

        string adminPassword = "Admin@123456";

        await CreateUserIfNotExistsAsync(admin, adminPassword, adminRoleName);

        #endregion

        #region Books

        await SeedBooksAndCopiesAsync();

        #endregion

        #region LendingListRecordsAndBorrowingRequests

        await SeedLendingRecordsAndBorrowingRequestsAsync();

        #endregion

        #region BorrowingTransactionsAndRelated

        await SeedBorrowingTransactionsAsync();
        await SeedBorrowingTransactionEventsAsync();
        await SeedBorrowingReviewsAsync();
        await SeedPointTransactionsAsync();

        #endregion

        #region RefreshTokens

        await SeedRefreshTokensAsync();

        #endregion
    }

    #region Helpers

    private async Task SeedBooksAndCopiesAsync()
    {
        var owners = await context.Students
            .Where(s => s.State == StudentState.Active)
            .Select(s => s.Id)
            .ToListAsync();
        if (owners.Count == 0)
        {
            owners = await context.Students.Select(s => s.Id).ToListAsync();
        }
        if (owners.Count == 0)
            return;

        if (await context.Books.AnyAsync())
        {
            await EnsureMinimumAvailableCopiesAsync(owners, minimumAvailableCopies: 50);
            return;
        }

        var booksToSeed = new (string Title, string ISBN, string Publisher, BookCategory Category, string Author, string CoverFile)[]
        {
            ("Sapiens", "9780062316097", "Harper", BookCategory.Nonfiction | BookCategory.Biography, "Yuval Noah", $"book1.jpg"),
            ("Design Patterns", "9780201633610", "Addison Wesley", BookCategory.Nonfiction | BookCategory.Science, "إريك غاما", $"book2.jpg"),
            ("The Martian", "9780804139021", "Crown", BookCategory.ScienceFiction | BookCategory.Fiction, "Andy Weir", $"book3.jpg"),
            ("هاري بوتر", "9780439554930", "Scholastic", BookCategory.Fantasy | BookCategory.ChildrenBooks, "J K Rowling", $"book4.jpg"),
            ("The Da Vinci Code", "9780307474278", "Anchor", BookCategory.Mystery | BookCategory.Thriller, "Dan Brown", $"book5.jpg"),
            ("Refactoring", "9780201485677", "Addison Wesley", BookCategory.Nonfiction | BookCategory.Science, "مارتن فلور", $"book6.jpg"),
            ("The Hobbit", "9780547928227", "Houghton Mifflin", BookCategory.Fantasy | BookCategory.Fiction, "ج ر ر تولكين", $"book7.jpg"),
            ("Dune", "9780441013593", "Ace Books", BookCategory.ScienceFiction | BookCategory.Fiction, "فرانك هربرت", $"book8.jpg"),
            ("The Shining", "9780307743657", "Anchor", BookCategory.Horror | BookCategory.Fiction, "ستيفن كينغ", $"book9.jpg"),
            ("Gone Girl", "9780307588371", "Crown", BookCategory.Thriller | BookCategory.Mystery, "جيليان فلين", $"book10.jpg"),
            ("Pride And Prejudice", "9780141439518", "Penguin Classics", BookCategory.Romance | BookCategory.Fiction, "Jane Austen", $"book11.jpg"),
            ("العادات الذرية", "9780735211292", "Avery", BookCategory.SelfHelp | BookCategory.Psychology, "James Clear", $"book12.jpg"),
            ("The Alchemist", "9780062315007", "HarperOne", BookCategory.Fiction | BookCategory.Philosophy, "Paulo Coelho", $"book13.jpg"),
            ("الكود النظيف", "9780132350884", "Prentice Hall", BookCategory.Nonfiction | BookCategory.Business, "روبرت مارتن", $"book14.jpg"),
            ("The Pragmatic Programmer", "9780201616224", "Addison Wesley", BookCategory.Nonfiction | BookCategory.Business, "أندرو هنت", $"book15.jpg"),
        };


        var rng = new Random();

        foreach (var seed in booksToSeed)
        {
            var createdBook = await CreateBookIfNotExistsAsync(
                title: seed.Title,
                isbn: seed.ISBN,
                publisher: seed.Publisher,
                category: seed.Category,
                author: seed.Author,
                coverImageFileName: seed.CoverFile);

            if (createdBook is null)
                continue;

            if (rng.NextDouble() < 0.2)
            {
                createdBook.MarkAsRejected();
                continue;//if rejectded we don't create copies for this book
            }

            if (rng.NextDouble() < 0.6)
            {
                createdBook.MarkAsAvailable();

                var copiesCount = rng.Next(1, 10);

                for (var i = 0; i < copiesCount; i++)
                {
                    var ownerId = owners[rng.Next(owners.Count)];
                    var condition = GetRandomBookCopyCondition(rng);

                    await CreateBookCopyIfNotExistsAsync(ownerId, createdBook.Id, condition);
                }
            }
        }

        await EnsureMinimumAvailableCopiesAsync(owners, minimumAvailableCopies: 50);
    }

    private async Task EnsureMinimumAvailableCopiesAsync(List<Guid> owners, int minimumAvailableCopies)
    {
        var availableBooks = await context.Books
            .Where(b => b.Status == BookStatus.Available)
            .Select(b => b.Id)
            .ToListAsync();

        if (availableBooks.Count == 0 || owners.Count == 0)
            return;

        var availableCopiesCount = await context.BookCopies
            .CountAsync(bc => bc.State == BookCopyState.Available && owners.Contains(bc.OwnerId));

        if (availableCopiesCount >= minimumAvailableCopies)
            return;

        var rng = new Random();
        var copiesToCreate = minimumAvailableCopies - availableCopiesCount;

        for (var i = 0; i < copiesToCreate; i++)
        {
            var ownerId = owners[rng.Next(owners.Count)];
            var bookId = availableBooks[rng.Next(availableBooks.Count)];
            var condition = GetRandomBookCopyCondition(rng);

            await CreateBookCopyIfNotExistsAsync(ownerId, bookId, condition);
        }
    }

    private async Task SeedLendingRecordsAndBorrowingRequestsAsync()
    {
        await SeedLendingListRecordsAsync();
        await SeedBorrowingRequestsAsync();
    }

    private async Task SeedLendingListRecordsAsync()
    {
        if (await context.LendingListRecords.AnyAsync())
            return;

        var activeOwnerIds = await context.Students
            .Where(s => s.State == StudentState.Active)
            .Select(s => s.Id)
            .ToListAsync();

        if (activeOwnerIds.Count == 0)
            return;

        var desiredStates = new List<LendingListRecordState>
        {
            LendingListRecordState.Available,
            LendingListRecordState.Reserved,
            LendingListRecordState.Borrowed,
            LendingListRecordState.Expired,
            LendingListRecordState.Closed,
            
            // Available records for requests
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            
            // Available records for transactions
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            
            // More available records for general listing
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available,
            LendingListRecordState.Available
        };

        var requiredRecordsCount = desiredStates.Count;

        var bookCopies = await context.BookCopies
            .Where(bc => activeOwnerIds.Contains(bc.OwnerId) && bc.State == BookCopyState.Available)
            .OrderBy(bc => bc.CreatedAtUtc)
            .Take(requiredRecordsCount)
            .ToListAsync();

        if (bookCopies.Count < requiredRecordsCount)
            return;

        var now = DateTimeOffset.UtcNow;

        for (var i = 0; i < desiredStates.Count; i++)
        {
            var bookCopy = bookCopies[i];
            var recordResult = LendingListRecord.Create(
                Guid.NewGuid(),
                bookCopy.Id,
                borrowingDurationInDays: 14,
                cost: new Point(LendingListRecord.DefaultCostInPoints),
                expirationDateUtc: now.AddDays(LendingListRecord.DefaultExpirationDurationInDays),
                currentTime: now);

            if (recordResult.IsFailure)
            {
                logger.LogError(
                    "Failed to create lending list record for book copy {BookCopyId}: {Errors}",
                    bookCopy.Id,
                    recordResult.Errors);
                continue;
            }

            var record = recordResult.Value;
            var desiredState = desiredStates[i];

            switch (desiredState)
            {
                case LendingListRecordState.Reserved:
                    if (record.MarkAsReserved().IsFailure)
                    {
                        logger.LogError("Failed to reserve lending list record for book copy {BookCopyId}", bookCopy.Id);
                        continue;
                    }
                    break;
                case LendingListRecordState.Borrowed:
                    if (
                         bookCopy.MarkAsBorrowed().IsFailure
                        || record.MarkAsReserved().IsFailure
                        || record.MarkAsBorrowed().IsFailure)
                    {
                        logger.LogError("Failed to borrow lending list record for book copy {BookCopyId}", bookCopy.Id);
                        continue;
                    }
                    break;
                case LendingListRecordState.Expired:
                    if (record.MarkAsExpired().IsFailure)
                    {
                        logger.LogError("Failed to expire lending list record for book copy {BookCopyId}", bookCopy.Id);
                        continue;
                    }
                    record.ExpirationDateUtc = now.AddDays(-1);
                    break;
                case LendingListRecordState.Closed:
                    if (record.MarkAsClosed().IsFailure)
                    {
                        logger.LogError("Failed to close lending list record for book copy {BookCopyId}", bookCopy.Id);
                        continue;
                    }
                    break;
            }

            context.LendingListRecords.Add(record);
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedBorrowingRequestsAsync()
    {
        if (await context.BorrowingRequests.AnyAsync())
            return;

        var activeStudentIds = await context.Students
            .Where(s => s.State == StudentState.Active)
            .Select(s => s.Id)
            .ToListAsync();

        if (activeStudentIds.Count < 2)
            return;

        var lendingRecords = await context.LendingListRecords
            .Include(lr => lr.BookCopy)
            .Where(lr => lr.BookCopy != null)
            .ToListAsync();

        var availableRecords = lendingRecords
            .Where(lr => lr.State == LendingListRecordState.Available)
            .ToList();

        var reservedRecord = lendingRecords
            .FirstOrDefault(lr => lr.State == LendingListRecordState.Reserved);

        var expiredRecord = lendingRecords
            .FirstOrDefault(lr => lr.State == LendingListRecordState.Expired);

        if (availableRecords.Count < 10 || reservedRecord is null || expiredRecord is null)
            return;

        var now = DateTimeOffset.UtcNow;

        var requestSeeds = new List<(LendingListRecord Record, BorrowingRequestState State)>
        {
            (availableRecords[0], BorrowingRequestState.Pending),
            (availableRecords[1], BorrowingRequestState.Pending),
            (availableRecords[2], BorrowingRequestState.Pending),
            (reservedRecord, BorrowingRequestState.Accepted),
            (availableRecords[3], BorrowingRequestState.Rejected),
            (availableRecords[4], BorrowingRequestState.Rejected),
            (availableRecords[5], BorrowingRequestState.Cancelled),
            (availableRecords[6], BorrowingRequestState.Cancelled),
            (expiredRecord, BorrowingRequestState.Expired)
        };

        foreach (var seed in requestSeeds)
        {
            var ownerId = seed.Record.BookCopy!.OwnerId;
            var borrowerId = activeStudentIds.FirstOrDefault(id => id != ownerId);

            if (borrowerId == Guid.Empty)
                continue;

            var borrowingRequestResult = BorrowingRequest.Create(
                Guid.NewGuid(),
                borrowingStudentId: borrowerId,
                lendingRecordId: seed.Record.Id,
                expirationDateUtc: now.AddDays(BorrowingRequest.DefaultExpirationDays),
                currentTime: now);

            if (borrowingRequestResult.IsFailure)
            {
                logger.LogError(
                    "Failed to create borrowing request for lending record {LendingRecordId}: {Errors}",
                    seed.Record.Id,
                    borrowingRequestResult.Errors);
                continue;
            }

            context.BorrowingRequests.Add(borrowingRequestResult.Value);
            context.Entry(borrowingRequestResult.Value)
                .Property(nameof(BorrowingRequest.State))
                .CurrentValue = seed.State;

            if (seed.State == BorrowingRequestState.Expired)
            {
                borrowingRequestResult.Value.ExpirationDateUtc = now.AddDays(-1);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedBorrowingTransactionsAsync()
    {
        if (await context.BorrowingTransactions.AnyAsync())
            return;

        var activeStudentIds = await context.Students
            .Where(s => s.State == StudentState.Active)
            .Select(s => s.Id)
            .ToListAsync();

        if (activeStudentIds.Count < 2)
            return;

        var lendingRecords = await context.LendingListRecords
            .Include(lr => lr.BookCopy)
            .Where(lr => lr.BookCopy != null
                         && lr.BookCopy.State == BookCopyState.Available
                         && (lr.State == LendingListRecordState.Available || lr.State == LendingListRecordState.Reserved))
            .OrderBy(lr => lr.CreatedAtUtc)
            .Take(12)
            .ToListAsync();

        if (lendingRecords.Count < 12)
            return;

        var now = DateTimeOffset.UtcNow;

        var transactionSeeds = new[]
        {
            BorrowingTransactionState.Borrowed,
            BorrowingTransactionState.Returned,
            BorrowingTransactionState.Overdue,
            BorrowingTransactionState.Lost,
            BorrowingTransactionState.Borrowed,
            BorrowingTransactionState.Returned,
            BorrowingTransactionState.Overdue,
            BorrowingTransactionState.Lost,
            BorrowingTransactionState.Returned,
            BorrowingTransactionState.Returned,
            BorrowingTransactionState.Borrowed,
            BorrowingTransactionState.Overdue
        };

        var transactions = new List<BorrowingTransaction>();

        for (var i = 0; i < lendingRecords.Count; i++)
        {
            var record = lendingRecords[i];
            var bookCopy = record.BookCopy!;

            var lenderId = bookCopy.OwnerId;
            var borrowerId = activeStudentIds.FirstOrDefault(id => id != lenderId);

            if (borrowerId == Guid.Empty)
                continue;

            if (record.State == LendingListRecordState.Available)
            {
                if (record.MarkAsReserved().IsFailure || record.MarkAsBorrowed().IsFailure)
                    continue;
            }
            else if (record.State == LendingListRecordState.Reserved)
            {
                if (record.MarkAsBorrowed().IsFailure)
                    continue;
            }

            var requestResult = BorrowingRequest.Create(
                Guid.NewGuid(),
                borrowingStudentId: borrowerId,
                lendingRecordId: record.Id,
                expirationDateUtc: now.AddDays(BorrowingRequest.DefaultExpirationDays),
                currentTime: now);

            if (requestResult.IsFailure)
                continue;

            context.BorrowingRequests.Add(requestResult.Value);
            context.Entry(requestResult.Value)
                .Property(nameof(BorrowingRequest.State))
                .CurrentValue = BorrowingRequestState.Accepted;

            var transactionResult = BorrowingTransaction.Create(
                Guid.NewGuid(),
                borrowingRequestId: requestResult.Value.Id,
                lenderStudentId: lenderId,
                borrowerStudentId: borrowerId,
                bookCopyId: bookCopy.Id,
                expectedReturnDate: now.AddDays(14),
                currentTime: now);

            if (transactionResult.IsFailure)
                continue;

            context.Entry(bookCopy)
                .Property(nameof(BookCopy.State))
                .CurrentValue = BookCopyState.Borrowed;

            transactions.Add(transactionResult.Value);
            context.BorrowingTransactions.Add(transactionResult.Value);
        }

        await context.SaveChangesAsync();

        for (var i = 0; i < transactions.Count && i < transactionSeeds.Length; i++)
        {
            var transaction = transactions[i];
            var desiredState = transactionSeeds[i];

            switch (desiredState)
            {
                case BorrowingTransactionState.Returned:
                {
                    var returnDate = transaction.CreatedAtUtc.AddDays(7);
                    if (transaction.ReturnBookCopy(returnDate, returnDate).IsSuccess)
                    {
                        var bookCopy = await context.BookCopies.FirstAsync(bc => bc.Id == transaction.BookCopyId);
                        bookCopy.MarkAsAvilable();
                    }
                    break;
                }
                case BorrowingTransactionState.Overdue:
                {
                    var overdueTime = transaction.CreatedAtUtc.AddDays(21);
                    transaction.MarkAsOverdue(overdueTime);
                    break;
                }
                case BorrowingTransactionState.Lost:
                {
                    if (transaction.MarkAsLost().IsSuccess)
                    {
                        var bookCopy = await context.BookCopies.FirstAsync(bc => bc.Id == transaction.BookCopyId);
                        bookCopy.MarkAsLost();
                    }
                    break;
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedBorrowingTransactionEventsAsync()
    {
        if (await context.BorrowingTransactionEvents.AnyAsync())
            return;

        var transactions = await context.BorrowingTransactions
            .OrderBy(t => t.CreatedAtUtc)
            .ToListAsync();

        if (transactions.Count == 0)
            return;

        foreach (var transaction in transactions)
        {
            var borrowedEvent = BorrowingTransactionEvent.Create(
                Guid.NewGuid(),
                transaction.Id,
                BorrowingTransactionState.Borrowed);

            if (borrowedEvent.IsSuccess)
                context.BorrowingTransactionEvents.Add(borrowedEvent.Value);

            if (transaction.State != BorrowingTransactionState.Borrowed)
            {
                var finalEvent = BorrowingTransactionEvent.Create(
                    Guid.NewGuid(),
                    transaction.Id,
                    transaction.State);

                if (finalEvent.IsSuccess)
                    context.BorrowingTransactionEvents.Add(finalEvent.Value);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedBorrowingReviewsAsync()
    {
        if (await context.BorrowingReviews.AnyAsync())
            return;

        var transactions = await context.BorrowingTransactions
            .Where(t => t.State == BorrowingTransactionState.Returned || t.State == BorrowingTransactionState.Overdue)
            .OrderBy(t => t.CreatedAtUtc)
            .ToListAsync();

        if (transactions.Count == 0)
            return;

        foreach (var transaction in transactions)
        {
            var isOverdue = transaction.State == BorrowingTransactionState.Overdue;

            var reviewerId = isOverdue ? transaction.LenderStudentId : transaction.BorrowerStudentId;
            var reviewedId = isOverdue ? transaction.BorrowerStudentId : transaction.LenderStudentId;

            // Create a FRESH StarsRating instance for each review.
            // EF Core owned types cannot be shared across multiple parent entities —
            // reusing the same StarsRating object causes EF to generate INSERTs without the Rating column.
            var ratingResult = StarsRating.Create(isOverdue ? 2 : 5);
            if (ratingResult.IsFailure)
                continue;

            var description = isOverdue ? "Returned late" : "Smooth borrowing and return";

            var reviewResult = BorrowingReview.Create(
                Guid.NewGuid(),
                reviewerId,
                reviewedId,
                transaction.Id,
                description,
                ratingResult.Value);

            if (reviewResult.IsFailure)
                continue;

            // Clear tracker to avoid cross-entity owned-type tracking conflicts
            context.ChangeTracker.Clear();
            context.BorrowingReviews.Add(reviewResult.Value);
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedPointTransactionsAsync()
    {
        if (await context.PointTransactions.AnyAsync())
            return;

        var reviews = await context.BorrowingReviews
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync();

        if (reviews.Count == 0)
            return;

        foreach (var review in reviews)
        {
            var isBadReview = review.Rating.Value <= 2;
            var reason = isBadReview ? PointTransactionReason.BadReview : PointTransactionReason.GoodReview;
            var points = isBadReview ? 2 : 5;

            var reviewedPointsResult = PointTransaction.Create(
                Guid.NewGuid(),
                review.ReviewedStudentId,
                review.Id,
                points,
                reason);

            if (reviewedPointsResult.IsSuccess)
                context.PointTransactions.Add(reviewedPointsResult.Value);

            var reviewerReason = PointTransactionReason.Reward;

            var reviewerPointsResult = PointTransaction.Create(
                Guid.NewGuid(),
                review.ReviewerStudentId,
                null,
                1,
                reviewerReason);

            if (reviewerPointsResult.IsSuccess)
                context.PointTransactions.Add(reviewerPointsResult.Value);
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedRefreshTokensAsync()
    {
        if (await context.RefreshTokens.AnyAsync())
            return;

        var users = await context.Users
            .OrderBy(u => u.Email)
            .Take(2)
            .ToListAsync();

        if (users.Count == 0)
            return;

        var now = DateTimeOffset.UtcNow;

        foreach (var user in users)
        {
            var tokenResult = RefreshToken.Create(
                Guid.NewGuid(),
                token: Guid.NewGuid().ToString("N"),
                userId: user.Id,
                expiresOnUtc: now.AddDays(30));

            if (tokenResult.IsFailure)
                continue;

            context.RefreshTokens.Add(tokenResult.Value);
        }

        await context.SaveChangesAsync();
    }

    private static BookCopyCondition GetRandomBookCopyCondition(Random rng)
    {
        var values = Enum.GetValues<BookCopyCondition>();
        return values[rng.Next(values.Length)];
    }

    private async Task<Book?> CreateBookIfNotExistsAsync(
        string title,
        string isbn,
        string publisher,
        BookCategory category,
        string author,
        string coverImageFileName)
    {
        if (await context.Books.AnyAsync(b => b.ISBN.Value == isbn))
            return await context.Books.FirstAsync(b => b.ISBN.Value == isbn);

        var titleResult = BookTitle.Create(title);
        if (titleResult.IsFailure)
        {
            logger.LogError("Failed to create book title for ISBN {ISBN} Errors {Errors}", isbn, titleResult.Errors);
            return null;
        }

        var isbnResult = ISBN.Create(isbn);
        if (isbnResult.IsFailure)
        {
            logger.LogError("Failed to create book ISBN for ISBN {ISBN} Errors {Errors}", isbn, isbnResult.Errors);
            return null;
        }

        var publisherResult = BookPublisher.Create(publisher);
        if (publisherResult.IsFailure)
        {
            logger.LogError("Failed to create book publisher for ISBN {ISBN} Errors {Errors}", isbn, publisherResult.Errors);
            return null;
        }

        var authorResult = BookAuthor.Create(author);
        if (authorResult.IsFailure)
        {
            logger.LogError("Failed to create book author for ISBN {ISBN} Errors {Errors}", isbn, authorResult.Errors);
            return null;
        }

        var bookResult = Book.Create(
            id: Guid.NewGuid(),
            title: titleResult.Value,
            isbn: isbnResult.Value,
            publisher: publisherResult.Value,
            category: category,
            author: authorResult.Value,
            coverImageFileName: coverImageFileName);

        if (bookResult.IsFailure)
        {
            logger.LogError("Failed to create book entity for ISBN {ISBN} Errors {Errors}", isbn, bookResult.Errors);
            return null;
        }

        context.Books.Add(bookResult.Value);
        await context.SaveChangesAsync();

        logger.LogInformation("Book seeded with ISBN {ISBN}", isbn);

        return bookResult.Value;
    }

    private async Task CreateBookCopyIfNotExistsAsync(Guid ownerId, Guid bookId, BookCopyCondition condition)
    {
        var exists = await context.BookCopies.AnyAsync(bc => bc.OwnerId == ownerId && bc.BookId == bookId);
        if (exists)
            return;

        var copyResult = BookCopy.Create(Guid.NewGuid(), ownerId, bookId, condition);

        if (copyResult.IsFailure)
        {
            logger.LogError("Failed to create book copy for BookId {BookId} OwnerId {OwnerId} Errors {Errors}", bookId, ownerId, copyResult.Errors);
            return;
        }

        context.BookCopies.Add(copyResult.Value);
        await context.SaveChangesAsync();
    }


    private async Task<bool> DatabaseAlreadyExistsAsync()
    {
        try
        {
            return await context.Database.CanConnectAsync();
        }
        catch (Exception ex) when (GetSqlException(ex)?.Number is 4060)
        {
            return false;
        }
    }

    private static bool IsTransientDatabaseStartupFailure(Exception ex)
    {
        var sqlException = GetSqlException(ex);

        if (sqlException is null)
            return false;

        return sqlException.Number is 4060 or 53 or -2 or 233 or 18456;
    }

    private static Microsoft.Data.SqlClient.SqlException? GetSqlException(Exception ex)
    {
        if (ex is Microsoft.Data.SqlClient.SqlException sqlException)
            return sqlException;

        return ex.InnerException as Microsoft.Data.SqlClient.SqlException;
    }

    private async Task CreateStudentIfNotExistsAsync(AppUser user,int index)
    {
        // check if student already exists
        var existingStudent = await context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
        if (existingStudent is not null)
        {
            var now = DateTimeOffset.UtcNow;

            switch (existingStudent.State)
            {
                case StudentState.Active:
                    return;
                case StudentState.Approved:
                    if (existingStudent.MarkAsActivated().IsFailure)
                        logger.LogError("Failed to activate existing student {Email}", user.Email);
                    break;
                case StudentState.Pending:
                    if (existingStudent.MarkAsApproved(now).IsFailure || existingStudent.MarkAsActivated().IsFailure)
                        logger.LogError("Failed to approve/activate existing student {Email}", user.Email);
                    break;
                case StudentState.Rejected:
                    if (existingStudent.MarkAsPend().IsFailure
                        || existingStudent.MarkAsApproved(now).IsFailure
                        || existingStudent.MarkAsActivated().IsFailure)
                        logger.LogError("Failed to reinstate existing student {Email}", user.Email);
                    break;
                case StudentState.Banned:
                    if (existingStudent.MarkAsUnBanned().IsFailure || existingStudent.MarkAsActivated().IsFailure)
                        logger.LogError("Failed to unban/activate existing student {Email}", user.Email);
                    break;
                case StudentState.UnBanned:
                    if (existingStudent.MarkAsActivated().IsFailure)
                        logger.LogError("Failed to activate existing student {Email}", user.Email);
                    break;
            }

            await context.SaveChangesAsync();
            return;
        }

        var nameResult = StudentName.Create(user.UserName!);
        var emailResult = UniversityMail.Create(user.Email!);
        string personalPhoto =$"student{index}.jpg";
        var telegramUserIdResult = TelegramUserId.Create($"student{index}");
        var phoneNumberResult = PhoneNumber.Create($"0109690981{index}");

        if (nameResult.IsFailure)
        {
            logger.LogError("Failed to create student value objects for user {Email} Errro {Error}", user.Email,nameResult.Errors);
            return;
        }

        if (emailResult.IsFailure)
        {
            logger.LogError("Failed to create student value objects for user {Email} Errro {Error}", user.Email, emailResult.Errors);
            return;
        }


        if (telegramUserIdResult.IsFailure)
        {
            logger.LogError("Failed to create student value objects for user {Email} Errro {Error}", user.Email, telegramUserIdResult.Errors);
            return;
        }

        if (phoneNumberResult.IsFailure)
        {
            logger.LogError("Failed to create student value objects for user {Email} Errro {Error}", user.Email, phoneNumberResult.Errors);
            return;
        }


        var studentResult = Student.Create(
            Guid.NewGuid(),
            nameResult.Value,
            emailResult.Value,
            personalPhoto,
            user.Id,
            phoneNumber: phoneNumberResult.Value,
            telegramUserId: telegramUserIdResult.Value
        );

        if (studentResult.IsFailure)
        {
            logger.LogError("Failed to create student entity for user {Email}", user.Email);
            return;
        }


        var approveResult = studentResult.Value.MarkAsApproved(DateTimeOffset.UtcNow);
        if (approveResult.IsFailure)
        {
            logger.LogError("Failed to approve student {Email}", user.Email);
            return;
        }

        var activateResult = studentResult.Value.MarkAsActivated();
        if (activateResult.IsFailure)
        {
            logger.LogError("Failed to activate student {Email}", user.Email);
            return;
        }

        context.Students.Add(studentResult.Value);
        await context.SaveChangesAsync();


        logger.LogInformation("Student created for user {Email}", user.Email);
    }

    private async Task<AppUser?> CreateUserIfNotExistsAsync(AppUser user, string password, string role)
    {
        var existingUser = await userManager.FindByEmailAsync(user.Email!);

        if (existingUser != null)
        {
            logger.LogInformation("User {Email} already exists", user.Email);
            return existingUser;
        }

        user.Id = Guid.NewGuid().ToString();

        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            logger.LogError("Failed to create user {Email}: {Errors}",
                user.Email,
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return null;
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            logger.LogError("Failed to assign role {Role} to user {Email}: {Errors}",
                role,
                user.Email,
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        logger.LogInformation("User {Email} created with role {Role}", user.Email, role);

        return user;
    }
    #endregion
}

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<AppDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}
