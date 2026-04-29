namespace BookOrbit.Application.SubcutaneousTests.Books.TestDoubles;

using BookOrbit.Application.Common.Interfaces;

public class FakeCurrentUser : ICurrentUser
{
    public string? Id { get; set; } = "admin-1";
    public string? Email { get; set; } = "admin@bookorbit.com";
    public bool IsInRole(string role) => true;
}
