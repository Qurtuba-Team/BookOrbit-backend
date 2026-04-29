namespace BookOrbit.Application.SubcutaneousTests.Identity.Queries;

using System.Security.Claims;
using BookOrbit.Application.Features.Identity.Queries.GenerateTokens;
using BookOrbit.Application.Features.Identity.Queries.GetUserById;
using BookOrbit.Application.Features.Identity.Queries.RefreshTokens;
using BookOrbit.Application.SubcutaneousTests.Identity.TestDoubles;
using BookOrbit.Application.SubcutaneousTests.Students;
using BookOrbit.Domain.Identity;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class IdentityQueriesSubcutaneousTests
{
    [Fact]
    public async Task GetUserByIdQuery_ShouldReturnUser()
    {
        // Arrange
        var identityService = new FakeIdentityService();
        var handler = new GetUserByIdQueryHandler(
            identityService,
            NullLogger<GetUserByIdQueryHandler>.Instance);

        var query = new GetUserByIdQuery("user-1");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be("user-1");
    }

    [Fact]
    public async Task GenerateTokenQuery_ShouldReturnTokens()
    {
        // Arrange
        var identityService = new FakeIdentityService();
        var tokenProvider = new FakeTokenProvider();
        var handler = new GenerateTokenQueryHandler(
            NullLogger<GenerateTokenQueryHandler>.Instance,
            tokenProvider,
            identityService);

        var query = new GenerateTokenQuery("user@std.mans.edu.eg", "pass");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access");
        result.Value.RefreshToken.Should().Be("refresh");
    }

    [Fact]
    public async Task RefreshTokenQuery_ShouldReturnTokens_WhenRefreshTokenValid()
    {
        // Arrange
        using var context = StudentTestFactory.CreateDbContext();
        var identityService = new FakeIdentityService();
        var tokenProvider = new FakeTokenProvider();
        var userId = "user-1";

        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "test"));

        tokenProvider.Principal = principal;

        var refreshToken = RefreshToken.Create(
            Guid.NewGuid(),
            "refresh-token",
            userId,
            DateTimeOffset.UtcNow.AddMinutes(10)).Value;

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        var handler = new RefreshTokenQueryHandler(
            NullLogger<RefreshTokenQueryHandler>.Instance,
            tokenProvider,
            identityService,
            context);

        var query = new RefreshTokenQuery("refresh-token", "expired-access");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access");
        result.Value.RefreshToken.Should().Be("refresh");
    }
}
