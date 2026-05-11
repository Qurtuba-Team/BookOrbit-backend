namespace BookOrbit.Infrastructure.Services.OutboxMessageServices;
public class OutboxMessageService
    (IAppDbContext context,
    ISerializationService serializationService,
    ILogger<OutboxMessageService> logger): IOutboxMessageService
{
#pragma warning disable CS1998 
    public async Task<Result<Success>> AddOutboxMessageAsync(OutboxMessageRecord payload, CancellationToken ct)
    {
        var serializedPayload = serializationService.Serialize(payload);
        
        if(serializedPayload.IsFailure)
        {
            logger.LogError("Failed to serialize outbox message payload. Errors: {Errors}", serializedPayload.Errors);
            return InfrastructureOutboxMessageErrors.SerializationFailed;
        }

        var OutboxMessageEntity = OutboxMessage.Create(
            Guid.NewGuid(),
            serializedPayload.Value);

        context.OutboxMessages.Add(OutboxMessageEntity.Value);
        return Result.Success;
    }
}