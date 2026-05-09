namespace BookOrbit.Application.Common.Interfaces;
public interface ISerializationService
{
    Result<string> Serialize<T>(T obj);
    Result<T> Deserialize<T>(string serializedObj);
}