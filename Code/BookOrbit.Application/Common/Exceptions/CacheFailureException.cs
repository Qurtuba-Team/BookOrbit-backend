namespace BookOrbit.Application.Common.Exceptions;
public sealed class CacheFailureException<T>(T result) : Exception
{
    public T Result { get; } = result;
}