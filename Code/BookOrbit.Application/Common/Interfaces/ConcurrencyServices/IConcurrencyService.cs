namespace BookOrbit.Application.Common.Interfaces.ConcurrencyServices;

public interface IConcurrencyService
{
    Result<Success> SetOriginalRowVersion(
      IConcurrencyEntity entity,
      string originalRowVersion);
}