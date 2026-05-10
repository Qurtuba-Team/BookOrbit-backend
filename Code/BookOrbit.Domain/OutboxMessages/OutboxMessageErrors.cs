namespace BookOrbit.Domain.OutboxMessages;
static public class OutboxMessageErrors
{
    private const string ClassName = nameof(OutboxMessage);
    static public readonly Error IdRequired = DomainCommonErrors.RequiredProp(ClassName,"Id","Id");
    static public readonly Error PayloadRequired = DomainCommonErrors.RequiredProp(ClassName,"Payload","Payload");
    public static Error InvalidStateTransition(OutboxMessageState currentState, OutboxMessageState newState)
    {
        if (currentState == newState)
            return DomainCommonErrors.InvalidStateTransitionSameState(ClassName, currentState.ToString());

        return DomainCommonErrors.InvalidStateTransition(ClassName, currentState.ToString(), newState.ToString());
    }
}