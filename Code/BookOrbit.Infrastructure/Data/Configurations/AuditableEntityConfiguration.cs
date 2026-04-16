namespace BookOrbit.Infrastructure.Data.Configurations
{
    public static class AuditableEntityConfiguration
    {
        public static void ConfigureAuditable<T>(this EntityTypeBuilder<T> builder)
            where T : AuditableEntity
        {
            builder.Property(e => e.CreatedAtUtc)
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(e => e.CreatedBy)
                .HasMaxLength(256)
                .IsRequired(false);

            builder.Property(e => e.LastModifiedUtc)
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(e => e.LastModifiedBy)
                .HasMaxLength(256)
                .IsRequired(false);
        }
    }
}