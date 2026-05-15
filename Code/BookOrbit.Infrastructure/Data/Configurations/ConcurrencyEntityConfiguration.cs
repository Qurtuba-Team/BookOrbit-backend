namespace BookOrbit.Infrastructure.Data.Configurations;
public static class ConcurrencyEntityConfiguration
{
    public static void ConfigureConcurrency<T>(this EntityTypeBuilder<T> builder)
            where T : class, IConcurrencyEntity
    {
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}