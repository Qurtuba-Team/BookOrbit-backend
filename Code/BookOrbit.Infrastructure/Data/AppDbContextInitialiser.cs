using BookOrbit.Domain.BookCopies.Enums;
using BookOrbit.Domain.Books.Enums;
using BookOrbit.Domain.Books.ValueObjects;

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
                UserName = "student1@std.mans.edu.eg",
                EmailConfirmed = true
            },
            new()
            {
                Email = "student2@std.mans.edu.eg",
                UserName = "student2@std.mans.edu.eg",
                EmailConfirmed = true
            },
            new()
            {
                Email = "student3@std.mans.edu.eg",
                UserName = "student3@std.mans.edu.eg",
                EmailConfirmed = true
            },
            new()
            {
                Email = "student4@std.mans.edu.eg",
                UserName = "student4@std.mans.edu.eg",
                EmailConfirmed = true
            },
            new()
            {
                Email = "student5@std.mans.edu.eg",
                UserName = "student5@std.mans.edu.eg",
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
            UserName = "admin@bookorbit.com",
            EmailConfirmed = true
        };

        string adminPassword = "Admin@123456";

        await CreateUserIfNotExistsAsync(admin, adminPassword, adminRoleName);

        #endregion

        #region Books

        await SeedBooksAndCopiesAsync();

        #endregion

        #region Interests

        await SeedInterestsAsync();

        #endregion
    }

    #region Helpers

    private async Task SeedInterestsAsync()
    {
        if (await context.Interests.AnyAsync())
            return;

        var interests = Enum.GetValues<BookOrbit.Application.Common.Enums.InterestType>()
            .Select(it => new BookOrbit.Domain.Common.Entities.Interest
            {
                Type = (int)it,
                Name = it.ToString()
            })
            .ToList();

        await context.Interests.AddRangeAsync(interests);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} interest types.", interests.Count);
    }

    private async Task SeedBooksAndCopiesAsync()
    {
        if (await context.Books.AnyAsync())
            return;

        var owners = await context.Students.Select(s => s.Id).ToListAsync();
        if (owners.Count == 0)
            return;

        var booksToSeed = new (string Title, string ISBN, string Publisher, BookCategory Category, string Author, string CoverFile)[]
        {
            ("الكود النظيف", "9780132350884", "Prentice Hall", BookCategory.Nonfiction | BookCategory.Business, "روبرت مارتن", $"{Guid.NewGuid()}.png"),
            ("The Pragmatic Programmer", "9780201616224", "Addison Wesley", BookCategory.Nonfiction | BookCategory.Business, "أندرو هنت", $"{Guid.NewGuid()}.png"),
            ("Design Patterns", "9780201633610", "Addison Wesley", BookCategory.Nonfiction | BookCategory.Science, "إريك غاما", $"{Guid.NewGuid()}.png"),
            ("Refactoring", "9780201485677", "Addison Wesley", BookCategory.Nonfiction | BookCategory.Science, "مارتن فلور", $"{Guid.NewGuid()}.png"),
            ("The Hobbit", "9780547928227", "Houghton Mifflin", BookCategory.Fantasy | BookCategory.Fiction, "ج ر ر تولكين", $"{Guid.NewGuid()}.png"),
            ("Dune", "9780441013593", "Ace Books", BookCategory.ScienceFiction | BookCategory.Fiction, "فرانك هربرت", $"{Guid.NewGuid()}.png"),
            ("The Shining", "9780307743657", "Anchor", BookCategory.Horror | BookCategory.Fiction, "ستيفن كينغ", $"{Guid.NewGuid()}.png"),
            ("Gone Girl", "9780307588371", "Crown", BookCategory.Thriller | BookCategory.Mystery, "جيليان فلين", $"{Guid.NewGuid()}.png"),
            ("Pride And Prejudice", "9780141439518", "Penguin Classics", BookCategory.Romance | BookCategory.Fiction, "Jane Austen", $"{Guid.NewGuid()}.png"),
            ("Sapiens", "9780062316097", "Harper", BookCategory.Nonfiction | BookCategory.Biography, "Yuval Noah", $"{Guid.NewGuid()}.png"),
            ("العادات الذرية", "9780735211292", "Avery", BookCategory.SelfHelp | BookCategory.Psychology, "James Clear", $"{Guid.NewGuid()}.png"),
            ("The Alchemist", "9780062315007", "HarperOne", BookCategory.Fiction | BookCategory.Philosophy, "Paulo Coelho", $"{Guid.NewGuid()}.png"),
            ("The Martian", "9780804139021", "Crown", BookCategory.ScienceFiction | BookCategory.Fiction, "Andy Weir", $"{Guid.NewGuid()}.png"),
            ("هاري بوتر", "9780439554930", "Scholastic", BookCategory.Fantasy | BookCategory.ChildrenBooks, "J K Rowling", $"{Guid.NewGuid()}.png"),
            ("The Da Vinci Code", "9780307474278", "Anchor", BookCategory.Mystery | BookCategory.Thriller, "Dan Brown", $"{Guid.NewGuid()}.png")
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

            var copiesCount = rng.Next(1, 10);

            for (var i = 0; i < copiesCount; i++)
            {
                var ownerId = owners[rng.Next(owners.Count)];
                var condition = GetRandomBookCopyCondition(rng);

                await CreateBookCopyIfNotExistsAsync(ownerId, createdBook.Id, condition);
            }
        }
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
        if (await context.Students.AnyAsync(s => s.UserId == user.Id))
            return;

        var nameResult = StudentName.Create($"Student {index.ToWords(new CultureInfo("en"))} Name");
        var emailResult = UniversityMail.Create(user.Email!);
        string personalPhoto =$"{Guid.NewGuid()}.png";
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

        Random rand = new Random();

        if (rand.NextDouble() < 0.5)
        {
            //Activate the student
            studentResult.Value.MarkAsActivated();
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
