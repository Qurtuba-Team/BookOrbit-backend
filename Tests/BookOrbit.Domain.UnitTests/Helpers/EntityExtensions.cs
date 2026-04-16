using System.Reflection;

namespace BookOrbit.Domain.UnitTests.Helpers;
static public class EntityExtensions
{
    public static T SetCreatedAt<T>(this T entity, DateTimeOffset createdAtUtc) where T : class
    {
        var property = typeof(T).GetProperty("CreatedAtUtc", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (property != null && property.CanWrite)
        {
            property.SetValue(entity, createdAtUtc);
        }
        else
        {
            // look for backing field if property is not found or not writable
            var field = typeof(T).GetField("<CreatedAtUtc>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(entity, createdAtUtc);
        }

        return entity;
    }
}
