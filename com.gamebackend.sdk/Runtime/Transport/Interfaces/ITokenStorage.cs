namespace GameBackend.Transport.Interfaces
{
    /// <summary>Token storage abstraction. Default: PlayerPrefs.</summary>
    public interface ITokenStorage
    {
        void Save(string authToken, string refreshToken);
        (string authToken, string refreshToken) Load();
        void Clear();
    }
}
