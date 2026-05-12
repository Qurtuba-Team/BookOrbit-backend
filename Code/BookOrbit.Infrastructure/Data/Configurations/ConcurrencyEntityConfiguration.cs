namespace BookOrbit.Infrastructure.Data.Configurations;
public static class ConcurrencyEntityConfiguration
{
    public static void ConfigureAuditable<T>(this EntityTypeBuilder<T> builder)
            where T : ConcurrencyEntity
    {
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}