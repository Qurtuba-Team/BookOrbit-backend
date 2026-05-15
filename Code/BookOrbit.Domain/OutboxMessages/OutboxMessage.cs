namespace BookOrbit.Domain.OutboxMessages;
public class OutboxMessage : Entity, IAuditableEntity
{
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModifiedUtc { get; set; }
    public string? LastModifiedBy { get; set; }

    private OutboxMessage(
        Guid id,
        string payload) : base(id)
    {
        State = OutboxMessageState.Pending;
        Payload = payload;
        RetryCount = 0;
    }

    public OutboxMessageState State { get; private set; }
    public string Payload { get; private set; } = null!;
    public int RetryCount { get; private set; }


    static public Result<OutboxMessage> Create(Guid id,
                                               string payload)
    {
        if (id == Guid.Empty)
        {
            return OutboxMessageErrors.IdRequired;
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            return OutboxMessageErrors.PayloadRequired;
        }

        return new OutboxMessage(id, payload);
    }

    private bool CanTransitionTo(OutboxMessageState newState)
    {
        return State switch
        {
            OutboxMessageState.Pending => newState is OutboxMessageState.Processed or OutboxMessageState.Failed,
            OutboxMessageState.Processed => false,
            OutboxMessageState.Failed => newState == OutboxMessageState.Pending,
            _ => false
        };
    }

    private Result<Updated> UpdateState(OutboxMessageState newState)
    {
        if (!CanTransitionTo(newState))
        {
            return OutboxMessageErrors.InvalidStateTransition(State, newState);
        }
        State = newState;
        return Result.Updated;
    }

    public Result<Updated> MarkAsProcessed()
       =>UpdateState(OutboxMessageState.Processed);

    public Result<Updated> MarkAsFailed()
       => UpdateState(OutboxMessageState.Failed);

}