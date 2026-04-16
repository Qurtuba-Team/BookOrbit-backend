namespace BookOrbit.Application.Common.Constants;
static public class PoliciesNames
{
    // Only Users Who Confirmed Their Emails
    public const string ActiveUserPolicy = "ActiveUserAccess";

    // Only User Who Is Saved In The System (Authenticated)
    public const string RegisteredUserPolicy = "RegisteredUserAccess";

    // Only Users Who Are Saved In The System (Authenticated)
    // And Has The Same Id Of Resource's User ID
    public const string RegisteredUserOwnershipPolicy = "RegisteredUserOwnershipAccess";

    // Only Users With Role ["Admin"]
    public const string AdminOnlyPolicy = "AdminOnlyAccess";

    // Only Users With Role ["Admin"] 
    // Or Users With Role ["Student"] But Has The Same Id Of The Resource's student Id
    // Doesn't Have To be confirmed
    public const string StudentOwnershipPolicy = "StudentOwnerShipAccess";

    // No role Except Student, Even Admin
    public const string StudentOnlyPolicy = "StudentOnlyAccess";

    // Student With Active State
    public const string ActiveStudentPolicy = "ActiveStudentAccess";
}
