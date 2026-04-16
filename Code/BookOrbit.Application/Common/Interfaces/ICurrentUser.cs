namespace BookOrbit.Application.Common.Interfaces;
public interface ICurrentUser
{
    string? Id { get;}
    string? Email { get; }
    bool IsInRole(string role);
}
