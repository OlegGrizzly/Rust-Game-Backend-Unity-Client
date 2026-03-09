using GameBackend.Transport.Interfaces;

namespace GameBackend.Tests.Mocks
{
    public class MockTokenStorage : ITokenStorage
    {
        private string _authToken = "";
        private string _refreshToken = "";

        public string SavedAuthToken => _authToken;
        public string SavedRefreshToken => _refreshToken;
        public int SaveCallCount { get; private set; }
        public int ClearCallCount { get; private set; }

        public void Save(string authToken, string refreshToken)
        {
            _authToken = authToken;
            _refreshToken = refreshToken;
            SaveCallCount++;
        }

        public (string authToken, string refreshToken) Load()
        {
            return (_authToken, _refreshToken);
        }

        public void Clear()
        {
            _authToken = "";
            _refreshToken = "";
            ClearCallCount++;
        }
    }
}
