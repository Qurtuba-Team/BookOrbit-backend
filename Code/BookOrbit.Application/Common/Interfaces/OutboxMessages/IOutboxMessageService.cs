namespace BookOrbit.Application.Common.Interfaces.OutboxMessages;
public interface IOutboxMessageService
{
    Task<Result<Success>> AddOutboxMessageAsync(OutboxMessageRecord payload, CancellationToken ct);
}