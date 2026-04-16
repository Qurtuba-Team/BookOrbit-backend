namespace BookOrbit.Api.Common.Helpers;
public static class FlagEnumHelper
{
    public static T? Map<T>(List<T>? values) where T : struct, Enum
    {
        if (values is null || values.Count == 0)
            return null;

        long result = 0;

        foreach (T value in values)
        {
            result |= Convert.ToInt64(value);
        }

        return (T)Enum.ToObject(typeof(T), result);
    }
}