namespace BookOrbit.Api.Contracts.Requests.Common;

public abstract record ConcurrencyRequest
{
    public abstract string RowVersion { get; set; }
}