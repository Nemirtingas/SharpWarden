namespace SharpWarden.BitWardenDatabaseSession.Services;

public interface ISessionJsonConverterService
{
    T Deserialize<T>(string json);
    T Deserialize<T>(Stream json);
    string Serialize(object obj);
}