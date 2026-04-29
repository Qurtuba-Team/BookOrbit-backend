
namespace BookOrbit.Infrastructure.Data.Configurations;
public class LendingListRecordConfiguration : IEntityTypeConfiguration<LendingListRecord>
{
    public void Configure(EntityTypeBuilder<LendingListRecord> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("LendingListRecords");

        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.Id)
            .ValueGeneratedNever();

        builder.Property(lr => lr.BookCopyId)
            .IsRequired();

        builder.Property(lr => lr.BorrowingDurationInDays)
            .IsRequired();

        builder.Property(lr => lr.State)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(lr => lr.ExpirationDateUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.OwnsOne(lr => lr.Cost, c =>
        {
            c.Property(x => x.Value)
             .HasColumnName("Cost")
             .IsRequired();
        });

        builder.HasOne(lr => lr.BookCopy)
            .WithMany()
            .HasForeignKey(lr => lr.BookCopyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(lr => lr.BookCopyId);
        builder.HasIndex(lr => lr.State);
    }
}
