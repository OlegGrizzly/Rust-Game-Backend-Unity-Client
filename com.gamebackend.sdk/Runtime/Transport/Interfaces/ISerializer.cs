namespace GameBackend.Transport.Interfaces
{
    /// <summary>Serialization abstraction. Default: Newtonsoft.Json.</summary>
    public interface ISerializer
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string json);
    }
}
