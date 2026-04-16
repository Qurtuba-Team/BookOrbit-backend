namespace BookOrbit.Infrastructure.Data.Configurations;
public class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
{
    public void Configure(EntityTypeBuilder<BookCopy> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("BookCopies");

        builder.HasKey(bc => bc.Id);

        builder.Property(bc => bc.Id)
            .ValueGeneratedNever();

        builder.Property(bc => bc.OwnerId)
            .IsRequired();

        builder.Property(bc => bc.BookId)
            .IsRequired();

        builder.Property(bc => bc.Condition)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(bc => bc.State)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();


        builder.HasOne(bc => bc.Owner)
            .WithMany()
            .HasForeignKey(bc => bc.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bc => bc.Book)
            .WithMany() 
            .HasForeignKey(bc => bc.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bc => bc.OwnerId);
        builder.HasIndex(bc => bc.BookId);
        builder.HasIndex(bc => bc.State);
        builder.HasIndex(bc => bc.Condition);
    }
}