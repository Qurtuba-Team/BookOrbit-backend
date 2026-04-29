namespace BookOrbit.Application.SubcutaneousTests.Identity.TestDoubles;

using BookOrbit.Application.Common.Interfaces;

internal sealed class FakeRouteService : IRouteService
{
    public string GetEmailConfirmationRoute(string email, string token)
        => $"https://example.test/confirm?email={email}&token={token}";

    public string GetResetPasswordRoute(string email, string token)
        => $"https://example.test/reset?email={email}&token={token}";

    public string GetBookCoverImageRoute() => "https://example.test/books";

    public string GetBookCoverImageRoute(string fileName)
        => $"https://example.test/books/{fileName}";
}
