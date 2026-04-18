namespace BookOrbit.Domain.Common.Entities;

public class UserBookInteraction
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid BookId { get; set; }
    public int? Rating { get; set; }
    public bool IsRead { get; set; }
    public bool IsWishlisted { get; set; }
    public DateTime InteractionDate { get; set; }

    public Book Book { get; set; } = null!;
}
