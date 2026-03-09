using UnityEngine;
using GameBackend.Transport.Interfaces;

namespace GameBackend.Transport.Storage
{
    public class PlayerPrefsTokenStorage : ITokenStorage
    {
        const string AuthTokenKey = "gb_auth_token";
        const string RefreshTokenKey = "gb_refresh_token";

        public void Save(string authToken, string refreshToken)
        {
            PlayerPrefs.SetString(AuthTokenKey, authToken);
            PlayerPrefs.SetString(RefreshTokenKey, refreshToken);
            PlayerPrefs.Save();
        }

        public (string authToken, string refreshToken) Load()
        {
            return (
                PlayerPrefs.GetString(AuthTokenKey, ""),
                PlayerPrefs.GetString(RefreshTokenKey, "")
            );
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(AuthTokenKey);
            PlayerPrefs.DeleteKey(RefreshTokenKey);
            PlayerPrefs.Save();
        }
    }
}
