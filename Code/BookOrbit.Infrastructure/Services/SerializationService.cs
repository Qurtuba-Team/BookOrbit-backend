namespace BookOrbit.Infrastructure.Services;
internal class SerializationService(
    ILogger<SerializationService> logger) : ISerializationService
{


    
    public Result<T> Deserialize<T>(string serializedObj)
    {
        try
        {
            var deserializedObj = JsonSerializer.Deserialize<T>(serializedObj);

            if (deserializedObj is null)
            {
                logger.LogError("Deserialization resulted in null for object of type {Type}.", typeof(T).FullName);
                return Error.Failure();
            }

            return deserializedObj;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Deserialization failed for object of type {Type}: {Message}", typeof(T).FullName, ex.Message);
            return Error.Failure();
        }
    }

    public Result<string> Serialize<T>(T obj)
    {
        string serializedObj = string.Empty;
        try
        {
            return JsonSerializer.Serialize(obj);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Serialization failed for object of type {Type}: {Message}", typeof(T).FullName, ex.Message);
            return Error.Failure();
        }
    }
}