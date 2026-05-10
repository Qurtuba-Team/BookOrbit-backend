namespace BookOrbit.Domain.OutboxMessages.Enums;
public enum OutboxMessageState
{
    Pending = 0,
    Processed = 1,
    Failed = 2
}