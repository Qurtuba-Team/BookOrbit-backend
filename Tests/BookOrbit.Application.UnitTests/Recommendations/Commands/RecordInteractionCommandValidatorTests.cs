using FluentValidation.TestHelper;

namespace BookOrbit.Application.UnitTests.Recommendations.Commands;

public class RecordInteractionCommandValidatorTests
{
    private readonly RecordInteractionCommandValidator _sut = new();

    // ── Valid cases ──────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithValidBookIdAndRating_PassesValidation()
    {
        var command = new RecordInteractionCommand(
            UserId:       "user-1",
            BookId:       Guid.NewGuid(),
            Rating:       3,
            IsRead:       false,
            IsWishlisted: false);

        var result = _sut.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullRating_PassesValidation()
    {
        var command = new RecordInteractionCommand(
            UserId:       "user-1",
            BookId:       Guid.NewGuid(),
            Rating:       null,
            IsRead:       true,
            IsWishlisted: true);

        var result = _sut.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Validate_WithBoundaryRatings_PassesValidation(int rating)
    {
        var command = new RecordInteractionCommand(
            UserId: "u", BookId: Guid.NewGuid(), Rating: rating, IsRead: false, IsWishlisted: false);

        _sut.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ── Invalid cases ────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyBookId_FailsWithBookIdRequiredMessage()
    {
        var command = new RecordInteractionCommand(
            UserId:       "user-1",
            BookId:       Guid.Empty,
            Rating:       3,
            IsRead:       false,
            IsWishlisted: false);

        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.BookId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(100)]
    public void Validate_WithOutOfRangeRating_FailsValidation(int rating)
    {
        var command = new RecordInteractionCommand(
            UserId:       "user-1",
            BookId:       Guid.NewGuid(),
            Rating:       rating,
            IsRead:       false,
            IsWishlisted: false);

        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }
}
