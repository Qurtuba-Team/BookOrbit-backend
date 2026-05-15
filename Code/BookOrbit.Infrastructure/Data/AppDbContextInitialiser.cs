
using BookOrbit.Application.Features.BorrowingRequests.Commands.CreateBorrowingRequest;
using BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.AcceptBorrowingRequest;
using BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.CancelBorrowingRequest;
using BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.ExpireBorrowingRequest;
using BookOrbit.Application.Features.BorrowingRequests.Commands.StateMachien.RejectBorrowingRequest;
using BookOrbit.Application.Features.BorrowingReviews.Commands.CreateBorrowingReview;
using BookOrbit.Application.Features.BorrowingTransactions.Commands.CreateBorrowingTransaction;
using BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsLostBorrowingTransaction;
using BookOrbit.Application.Features.BorrowingTransactions.Commands.StateMachien.MarkAsReturnedBorrowingTransaction;
using BookOrbit.Application.Features.LendingListings.Commands.CreateLendingListRecord;
using BookOrbit.Application.Features.LendingListings.Commands.StateMachien.CloseLendingListRecord;
using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.LendingListings.Enums;

namespace BookOrbit.Infrastructure.Data;


public class AppDbContextInitialiser(
    ILogger<AppDbContextInitialiser> logger,
    UserManager<AppUser> userManager,
    AppDbContext context,
    RoleManager<IdentityRole> roleManager,
    ISender sender)
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

    #region Methods

    public async Task SeedRoles(string studentRoleName, string adminRoleName)
    {
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
    }
    public async Task SeedAdminAsync(string adminRoleName)
    {
        var admin = new AppUser
        {
            Email = "admin@bookorbit.com",
            UserName = "the admin",
            EmailConfirmed = true
        };

        string adminPassword = "Admin@123456";

        await CreateUserIfNotExistsAsync(admin, adminPassword, adminRoleName);
    }

    #region Students
    public async Task SeedStudents(string studentRoleName)
    {
        var studentsUserAccounts = new List<AppUser>
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

        foreach (var studentUser in studentsUserAccounts)
        {
            var user = await CreateUserIfNotExistsAsync(studentUser, studentPassword, studentRoleName);

            if (user is null)
                continue;


            await CreateStudentIfNotExistsAsync(user, index);
            index++;
        }
    }
    private async Task<AppUser?> CreateUserIfNotExistsAsync(AppUser user, string password, string role)
    {
        var existingUser = await userManager.FindByEmailAsync(user.Email!);

        if (existingUser is not null)
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
    private async Task CreateStudentIfNotExistsAsync(AppUser user, int index)
    {
        // check if student already exists
        var existingStudent = await context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

        if (existingStudent is not null)
            return;

        var nameResult = StudentName.Create(user.UserName!);
        var emailResult = UniversityMail.Create(user.Email!);
        string personalPhoto = $"student{index}.jpg";
        var telegramUserIdResult = TelegramUserId.Create($"student{index}");
        var phoneNumberResult = PhoneNumber.Create($"0109690981{index}");

        if (nameResult.IsFailure)
        {
            logger.LogError("Failed to create student value objects for user {Email} Errro {Error}", user.Email, nameResult.Errors);
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
    #endregion

    #region Books
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
                await context.SaveChangesAsync();
                continue;//if rejectded we don't create copies for this book
            }

            if (rng.NextDouble() < 0.6)
            {
                createdBook.MarkAsAvailable();
                await context.SaveChangesAsync();

                var copiesCount = rng.Next(1, 10);

                for (var i = 0; i < copiesCount; i++)
                {
                    var ownerId = owners[rng.Next(owners.Count)];
                    var condition = GetRandomBookCopyCondition(rng);

                    await CreateBookCopyAsync(ownerId, createdBook.Id, condition);
                }
            }
        }

        await EnsureMinimumAvailableCopiesAsync(owners, minimumAvailableCopies: 50);
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

    private static BookCopyCondition GetRandomBookCopyCondition(Random rng)
    {
        var values = Enum.GetValues<BookCopyCondition>();
        return values[rng.Next(values.Length)];
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

            await CreateBookCopyAsync(ownerId, bookId, condition);
        }
    }
    private async Task CreateBookCopyAsync(Guid ownerId, Guid bookId, BookCopyCondition condition)
    {
        var copyResult = BookCopy.Create(Guid.NewGuid(), ownerId, bookId, condition);

        if (copyResult.IsFailure)
        {
            logger.LogError("Failed to create book copy for BookId {BookId} OwnerId {OwnerId} Errors {Errors}", bookId, ownerId, copyResult.Errors);
            return;
        }

        context.BookCopies.Add(copyResult.Value);
        await context.SaveChangesAsync();
    }
    #endregion

    #region Flow
    public async Task SeedFlow()
    {
        var students = await context.Students.Take(5).ToListAsync();

        if (students.Count < 2)
            return;

        var st1 = students[0];
        var st1Copies = await context.BookCopies.Where(bc => bc.OwnerId == st1.Id).ToListAsync();
        if (st1Copies.Count == 0)
            return;
        var st2 = students[1];
        await SeedReturnedTransaction(st1, st2, st1Copies[0]);
        await SeedBorrowingScenarios(students);
    }

    private async Task SeedReturnedTransaction(Student Lender, Student Borrower, BookCopy copy)
    {
        var requestResult = await CreateBorrowingRequestAsync(Lender, Borrower, copy, borrowingDurationInDays: 14);

        if (requestResult is null)
            return;

        var br = await context.BorrowingRequests.FirstOrDefaultAsync(br => br.Id == requestResult.Value.RequestId);
        var rv = br is null ? string.Empty : Convert.ToBase64String(br.RowVersion);
        var approveResult = await sender.Send(new AcceptBorrowingRequestCommand(requestResult.Value.RequestId, rv));

        if (approveResult.IsFailure)
        {
            logger.LogError("Failed to approve borrowing request {RequestId} for copy {CopyId} borrower {BorrowerId} Errors {Errors}", requestResult.Value.RequestId, copy.Id, Borrower.Id, approveResult.Errors);
            return;
        }

        var transactionResult = await sender.Send(new CreateBorrowingTransactionCommand(requestResult.Value.RequestId));

        if (transactionResult.IsFailure)
        {
            logger.LogError("Failed to create borrowing transaction for borrowing request {RequestId} for copy {CopyId} borrower {BorrowerId} Errors {Errors}", requestResult.Value.RequestId, copy.Id, Borrower.Id, transactionResult.Errors);
            return;
        }

        var returnResult = await sender.Send(new MarkAsReturnedBorrowingTransactionCommand(transactionResult.Value.Id, Convert.ToBase64String(transactionResult.Value.RowVersion ?? [])));

        if (returnResult.IsFailure)
        {
            logger.LogError("Failed to mark borrowing transaction {TransactionId} as returned for copy {CopyId} borrower {BorrowerId} Errors {Errors}", transactionResult.Value.Id, copy.Id, Borrower.Id, returnResult.Errors);
            return;
        }

        var reviewResult = await sender.Send(new CreateBorrowingReviewCommand(
            BorrowingTransactionId: transactionResult.Value.Id,
            Rating: 5,
            Description: "Great book, returned in perfect condition!"
            ));

        if (reviewResult.IsFailure)
        {
            logger.LogError("Failed to create review for borrowing transaction {TransactionId} for copy {CopyId} borrower {BorrowerId} Errors {Errors}", transactionResult.Value.Id, copy.Id, Borrower.Id, reviewResult.Errors);
            return;
        }

    }

    private async Task SeedBorrowingScenarios(List<Student> students)
    {
        if (students.Count < 5)
        {
            logger.LogWarning("Not enough students to seed borrowing scenarios.");
            return;
        }

        var activeListingCopyIds = await context.LendingListRecords
            .Where(lr => lr.State == LendingListRecordState.Available
                || lr.State == LendingListRecordState.Reserved
                || lr.State == LendingListRecordState.Borrowed)
            .Select(lr => lr.BookCopyId)
            .ToHashSetAsync();

        var usedCopyIds = new HashSet<Guid>();

        async Task<BookCopy?> GetAvailableCopyAsync(Student owner)
        {
            var copy = await context.BookCopies
                .Where(bc => bc.OwnerId == owner.Id
                    && bc.State == BookCopyState.Available
                    && !usedCopyIds.Contains(bc.Id)
                    && !activeListingCopyIds.Contains(bc.Id))
                .FirstOrDefaultAsync();

            if (copy is not null)
                usedCopyIds.Add(copy.Id);

            return copy;
        }

        var lender1 = students[0];
        var borrower1 = students[1];
        var borrower2 = students[2];
        var borrower3 = students[3];
        var borrower4 = students[4];

        var lostCopy = await GetAvailableCopyAsync(lender1);
        if (lostCopy is not null)
            await SeedLostTransaction(lender1, borrower2, lostCopy);

        var rejectedCopy = await GetAvailableCopyAsync(lender1);
        if (rejectedCopy is not null)
            await SeedRejectedBorrowingRequest(lender1, borrower3, rejectedCopy);

        var cancelledCopy = await GetAvailableCopyAsync(borrower1);
        if (cancelledCopy is not null)
            await SeedCancelledBorrowingRequest(borrower1, borrower4, cancelledCopy);

        var expiredCopy = await GetAvailableCopyAsync(borrower2);
        if (expiredCopy is not null)
            await SeedExpiredBorrowingRequest(borrower2, borrower4, expiredCopy);

        var closedCopy = await GetAvailableCopyAsync(borrower3);
        if (closedCopy is not null)
            await SeedClosedLendingListRecord(borrower3, closedCopy);
    }

    private async Task SeedLostTransaction(Student lender, Student borrower, BookCopy copy)
    {
        var requestResult = await CreateBorrowingRequestAsync(lender, borrower, copy, borrowingDurationInDays: 14);

        if (requestResult is null)
            return;

        var br2 = await context.BorrowingRequests.FirstOrDefaultAsync(br => br.Id == requestResult.Value.RequestId);
        var rv2 = br2 is null ? string.Empty : Convert.ToBase64String(br2.RowVersion);
        var approveResult = await sender.Send(new AcceptBorrowingRequestCommand(requestResult.Value.RequestId, rv2));

        if (approveResult.IsFailure)
        {
            logger.LogError("Failed to approve borrowing request {RequestId} for copy {CopyId} borrower {BorrowerId} Errors {Errors}", requestResult.Value.RequestId, copy.Id, borrower.Id, approveResult.Errors);
            return;
        }

        var transactionResult = await sender.Send(new CreateBorrowingTransactionCommand(requestResult.Value.RequestId));

        if (transactionResult.IsFailure)
        {
            logger.LogError("Failed to create borrowing transaction for borrowing request {RequestId} for copy {CopyId} borrower {BorrowerId} Errors {Errors}", requestResult.Value.RequestId, copy.Id, borrower.Id, transactionResult.Errors);
            return;
        }

        var lostResult = await sender.Send(new MarkAsLostBorrowingTransactionCommand(transactionResult.Value.Id, Convert.ToBase64String(transactionResult.Value.RowVersion ?? [])));

        if (lostResult.IsFailure)
        {
            logger.LogError("Failed to mark borrowing transaction {TransactionId} as lost for copy {CopyId} borrower {BorrowerId} Errors {Errors}", transactionResult.Value.Id, copy.Id, borrower.Id, lostResult.Errors);
            return;
        }
    }

    private async Task SeedRejectedBorrowingRequest(Student lender, Student borrower, BookCopy copy)
    {
        var requestResult = await CreateBorrowingRequestAsync(lender, borrower, copy, borrowingDurationInDays: 14);

        if (requestResult is null)
            return;

        var br3 = await context.BorrowingRequests.FirstOrDefaultAsync(br => br.Id == requestResult.Value.RequestId);
        var rv3 = br3 is null ? string.Empty : Convert.ToBase64String(br3.RowVersion);
        var rejectResult = await sender.Send(new RejectBorrowingRequestCommand(requestResult.Value.RequestId, rv3));

        if (rejectResult.IsFailure)
        {
            logger.LogError("Failed to reject borrowing request {RequestId} for copy {CopyId} borrower {BorrowerId} Errors {Errors}", requestResult.Value.RequestId, copy.Id, borrower.Id, rejectResult.Errors);
            return;
        }
    }

    private async Task SeedCancelledBorrowingRequest(Student lender, Student borrower, BookCopy copy)
    {
        var requestResult = await CreateBorrowingRequestAsync(lender, borrower, copy, borrowingDurationInDays: 14);

        if (requestResult is null)
            return;

        var br4 = await context.BorrowingRequests.FirstOrDefaultAsync(br => br.Id == requestResult.Value.RequestId);
        var rv4 = br4 is null ? string.Empty : Convert.ToBase64String(br4.RowVersion);
        var cancelResult = await sender.Send(new CancelBorrowingRequestCommand(requestResult.Value.RequestId, rv4));

        if (cancelResult.IsFailure)
        {
            logger.LogError("Failed to cancel borrowing request {RequestId} for copy {CopyId} borrower {BorrowerId} Errors {Errors}", requestResult.Value.RequestId, copy.Id, borrower.Id, cancelResult.Errors);
            return;
        }
    }

    private async Task SeedExpiredBorrowingRequest(Student lender, Student borrower, BookCopy copy)
    {
        var requestResult = await CreateBorrowingRequestAsync(lender, borrower, copy, borrowingDurationInDays: 14);

        if (requestResult is null)
            return;

        var br5 = await context.BorrowingRequests.FirstOrDefaultAsync(br => br.Id == requestResult.Value.RequestId);
        var rv5 = br5 is null ? string.Empty : Convert.ToBase64String(br5.RowVersion);
        var expireResult = await sender.Send(new ExpireBorrowingRequestCommand(requestResult.Value.RequestId, rv5));

        if (expireResult.IsFailure)
        {
            logger.LogError("Failed to expire borrowing request {RequestId} for copy {CopyId} borrower {BorrowerId} Errors {Errors}", requestResult.Value.RequestId, copy.Id, borrower.Id, expireResult.Errors);
            return;
        }
    }

    private async Task SeedClosedLendingListRecord(Student lender, BookCopy copy)
    {
        if (copy.OwnerId != lender.Id)
        {
            logger.LogError("Lender {LenderId} does not own the book copy {CopyId}", lender.Id, copy.Id);
            return;
        }

        var listResult = await sender.Send(new CreateLendingListRecordCommand(copy.Id, 14));

        if (listResult.IsFailure)
        {
            logger.LogError("Failed to create lending list record for copy {CopyId} Errors {Errors}", copy.Id, listResult.Errors);
            return;
        }

        var closeResult = await sender.Send(new CloseLendingListRecordCommand(listResult.Value.Id));

        if (closeResult.IsFailure)
        {
            logger.LogError("Failed to close lending list record {LendingListRecordId} for copy {CopyId} Errors {Errors}", listResult.Value.Id, copy.Id, closeResult.Errors);
            return;
        }
    }

    private async Task<(Guid RequestId, Guid LendingListRecordId)?> CreateBorrowingRequestAsync(
        Student lender,
        Student borrower,
        BookCopy copy,
        int borrowingDurationInDays)
    {
        if (copy.OwnerId != lender.Id)
        {
            logger.LogError("Lender {LenderId} does not own the book copy {CopyId}", lender.Id, copy.Id);
            return null;
        }

        var listResult = await sender.Send(new CreateLendingListRecordCommand(copy.Id, borrowingDurationInDays));

        if (listResult.IsFailure)
        {
            logger.LogError("Failed to create lending list record for copy {CopyId} Errors {Errors}", copy.Id, listResult.Errors);
            return null;
        }

        var requestResult = await sender.Send(new CreateBorrowingRequestCommand(borrower.Id, listResult.Value.Id));

        if (requestResult.IsFailure)
        {
            logger.LogError("Failed to create borrowing request for copy {CopyId} borrower {BorrowerId} Errors {Errors}", copy.Id, borrower.Id, requestResult.Errors);
            return null;
        }

        return (requestResult.Value.Id, listResult.Value.Id);
    }

    #endregion

    #endregion




    public async Task TrySeedAsync()
    {
        var studentRoleName = nameof(IdentityRoles.student);
        var adminRoleName = nameof(IdentityRoles.admin);


        await SeedRoles(studentRoleName, adminRoleName);
        await SeedStudents(studentRoleName);
        await SeedAdminAsync(adminRoleName);
        await SeedBooksAndCopiesAsync();
        await SeedFlow();
    }
    #region Helpers
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