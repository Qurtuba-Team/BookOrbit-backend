namespace BookOrbit.Domain.UnitTests.ValueObjects;

using BookOrbit.Domain.Common.ValueObjects;
using FluentAssertions;
using Xunit;

public class UrlTests
{
    #region Constants

    private const string NormalizedUrl = "https://example.com";

    #endregion

    #region Test Helpers

    // (No additional helpers needed)

    #endregion

    #region Create Method - Success Tests

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("https://example.com/path")]
    [InlineData("https://example.com/path?query=1")]
    public void Create_WithValidUrl_ReturnsSuccessResult(string validUrl)
    {
        // Act
        var result = Url.Create(validUrl);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(validUrl.Trim());
    }

    [Theory]
    [InlineData("  https://example.com  ")]
    [InlineData("\thttps://example.com\t")]
    [InlineData("https://example.com ")]
    public void Create_WithUrlAndWhitespace_NormalizesAndReturnsSuccess(string urlWithWhitespace)
    {
        // Act
        var result = Url.Create(urlWithWhitespace);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(NormalizedUrl);
    }

    #endregion

    #region Create Method - Null/Empty Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("  \t  \n  ")]
    public void Create_WithNullOrEmptyUrl_ReturnsRequiredUrlError(string? url)
    {
        // Act
        var result = Url.Create(url);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(UrlErrors.RequiredUrl);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("example.com")]
    [InlineData("www.example.com")]
    [InlineData("/relative/path")]
    public void Create_WithNonAbsoluteUrl_ReturnsInvalidUrlError(string nonAbsoluteUrl)
    {
        // Act
        var result = Url.Create(nonAbsoluteUrl);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(UrlErrors.InvalidUrl);
    }

    [Theory]
    [InlineData("http://")]
    [InlineData("https://")]
    [InlineData("https://exa mple.com")]
    public void Create_WithMalformedUrl_ReturnsInvalidUrlError(string invalidUrl)
    {
        // Act
        var result = Url.Create(invalidUrl);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(UrlErrors.InvalidUrl);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Create_WithUrlContainingFragment_ReturnsSuccess()
    {
        // Arrange
        const string url = "https://example.com/path#section";

        // Act
        var result = Url.Create(url);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(url);
    }

    #endregion
}
