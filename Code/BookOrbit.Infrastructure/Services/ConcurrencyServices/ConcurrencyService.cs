namespace BookOrbit.Infrastructure.Services.ConcurrencyServices;

public class ConcurrencyService(
    AppDbContext context,
    ILogger<ConcurrencyService> logger)
    : IConcurrencyService
{
    public Result<Success> SetOriginalRowVersion(
        IConcurrencyEntity entity,
        string? originalRowVersion)
    {
        if (string.IsNullOrWhiteSpace(originalRowVersion))
            return InfrastrucureConcurrencyErrors.ConcurrencyTokenRequired;

        byte[] rowVersionBytes;

        try
        {
            rowVersionBytes =
                Convert.FromBase64String(originalRowVersion);
        }
        catch (FormatException ex)
        {
            logger.LogError(ex, "Invalid row version format. Value {Value}", originalRowVersion);
            return InfrastrucureConcurrencyErrors.InvalidConcurrencyFormat;
        }

        context.Entry(entity)
            .Property(e => e.RowVersion)
            .OriginalValue = rowVersionBytes;

        return Result.Success;
    }
}