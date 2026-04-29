namespace BookOrbit.Application.SubcutaneousTests.Books.TestDoubles;

using BookOrbit.Application.Common.Interfaces;

public class FakeRouteService : IRouteService
{
    public string GetEmailConfirmationRoute(string email, string token) => $"http://localhost/confirm?email={email}&token={token}";
    public string GetResetPasswordRoute(string email, string token) => $"http://localhost/reset?email={email}&token={token}";
    public string GetBookCoverImageRoute() => "http://localhost/images";
    public string GetBookCoverImageRoute(string fileName) => $"http://localhost/images/{fileName}";
}
