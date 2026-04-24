
using BookOrbit.Domain.Books.Enums;
using BookOrbit.Domain.Books.ValueObjects;

namespace BookOrbit.Infrastructure.Data.Configurations;
public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("Books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.Category)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(b => b.Status)
        .HasConversion<string>()
        .IsRequired()
        .HasDefaultValue(BookStatus.Pending)
        .HasMaxLength(20);

        builder.Property(b => b.CoverImageFileName)
            .HasMaxLength(255)
            .IsUnicode(false)
            .IsRequired();


        builder.OwnsOne(b => b.Title, t =>
        {
            t.Property(x => x.Value)
             .HasColumnName("Title")
             .HasMaxLength(BookTitle.MaxLength)
             .IsRequired();

            t.HasIndex(x => x.Value);
        });


        builder.OwnsOne(b => b.ISBN, i =>
        {
            i.WithOwner();

            i.Property(x => x.Value)
             .HasColumnName("ISBN")
             .HasMaxLength(ISBN.MaxLength)
             .IsUnicode(false)
             .IsRequired();

            i.HasIndex(x => x.Value).IsUnique();
        });


        builder.OwnsOne(b => b.Publisher, p =>
        {
            p.Property(x => x.Value)
             .HasColumnName("Publisher")
             .HasMaxLength(BookPublisher.MaxLength)
             .IsRequired();
        });


        builder.OwnsOne(b => b.Author, a =>
        {
            a.Property(x => x.Value)
             .HasColumnName("Author")
             .HasMaxLength(BookAuthor.MaxLength)
             .IsRequired();
        });
    }
}