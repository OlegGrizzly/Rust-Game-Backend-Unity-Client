namespace GameBackend.WebSocket
{
    public class GameSocketOptions
    {
        public string Host { get; set; }
        public string AccessToken { get; set; }
        public float HeartbeatInterval { get; set; } = 30f;
        public float PongTimeout { get; set; } = 10f;
        public bool ReconnectEnabled { get; set; } = true;
    }
}
