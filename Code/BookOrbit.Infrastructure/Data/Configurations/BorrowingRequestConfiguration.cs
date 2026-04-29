
namespace BookOrbit.Infrastructure.Data.Configurations;
public class BorrowingRequestConfiguration : IEntityTypeConfiguration<BorrowingRequest>
{
    public void Configure(EntityTypeBuilder<BorrowingRequest> builder)
    {
        builder.ConfigureAuditable();

        builder.ToTable("BorrowingRequests");

        builder.HasKey(br => br.Id);

        builder.Property(br => br.Id)
            .ValueGeneratedNever();

        builder.Property(br => br.BorrowingStudentId)
            .IsRequired();

        builder.Property(br => br.LendingRecordId)
            .IsRequired();

        builder.Property(br => br.State)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(br => br.ExpirationDateUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired(false);

        builder.HasOne(br => br.BorrowingStudent)
            .WithMany()
            .HasForeignKey(br => br.BorrowingStudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(br => br.LendingRecord)
            .WithMany()
            .HasForeignKey(br => br.LendingRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(br => br.BorrowingStudentId);
        builder.HasIndex(br => br.LendingRecordId);
        builder.HasIndex(br => br.State);
    }
}
