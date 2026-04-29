namespace BookOrbit.Domain.UnitTests.Identity;

using BookOrbit.Domain.Identity;
using FluentAssertions;
using Xunit;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "token-value";
        var userId = "user-123";
        var expiresOnUtc = DateTimeOffset.UtcNow.AddMinutes(5);

        // Act
        var result = RefreshToken.Create(id, token, userId, expiresOnUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.Token.Should().Be(token);
        result.Value.UserId.Should().Be(userId);
        result.Value.ExpiresOnUtc.Should().Be(expiresOnUtc);
    }

    [Fact]
    public void Create_WithEmptyId_ReturnsIdRequiredError()
    {
        // Act
        var result = RefreshToken.Create(Guid.Empty, "token", "user", DateTimeOffset.UtcNow.AddMinutes(5));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(RefreshTokenErrors.IdRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyToken_ReturnsTokenRequiredError(string? token)
    {
        // Act
        var result = RefreshToken.Create(Guid.NewGuid(), token, "user", DateTimeOffset.UtcNow.AddMinutes(5));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(RefreshTokenErrors.TokenRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyUserId_ReturnsUserIdRequiredError(string? userId)
    {
        // Act
        var result = RefreshToken.Create(Guid.NewGuid(), "token", userId, DateTimeOffset.UtcNow.AddMinutes(5));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(RefreshTokenErrors.UserIdRequired);
    }

    [Fact]
    public void Create_WithExpirationInPast_ReturnsInvalidExpirationDateError()
    {
        // Act
        var result = RefreshToken.Create(Guid.NewGuid(), "token", "user", DateTimeOffset.UtcNow.AddMinutes(-1));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(RefreshTokenErrors.InvalidExpirationDate);
    }
}
