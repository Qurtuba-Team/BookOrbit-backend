namespace BookOrbit.Domain.Books.Enums;

[Flags]
public enum BookCategory
{
    None = 0,
    Fiction = 1 << 0,
    Nonfiction = 1 << 1,
    Mystery = 1 << 2,
    Thriller = 1 << 3,
    Romance = 1 << 4,
    ScienceFiction = 1 << 5,
    Fantasy = 1 << 6,
    Horror = 1 << 7,
    HistoricalFiction = 1 << 8,
    Biography = 1 << 9,
    Autobiography = 1 << 10,
    SelfHelp = 1 << 11,
    Business = 1 << 12,
    Science = 1 << 13,
    Philosophy = 1 << 14,
    Psychology = 1 << 15,
    ReligionAndSpirituality = 1 << 16,
    Travel = 1 << 17,
    Cooking = 1 << 18,
    ChildrenBooks = 1 << 19
}