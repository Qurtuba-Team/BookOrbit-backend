namespace BookOrbit.Domain.Common.Entities;

public interface IConcurrencyEntity
{
    byte[] RowVersion { get;}
}
