using System;

namespace GameBackend.WebSocket
{
    public class ReconnectHandler
    {
        private readonly float _baseDelay;
        private readonly float _maxDelay;
        private readonly float _multiplier;
        private float _currentDelay;
        private bool _firstCall = true;

        public ReconnectHandler(float baseDelay = 1f, float maxDelay = 30f, float multiplier = 2f)
        {
            _baseDelay = baseDelay;
            _maxDelay = maxDelay;
            _multiplier = multiplier;
            _currentDelay = baseDelay;
        }

        public bool Enabled { get; set; } = true;

        public bool ShouldReconnect => Enabled;

        public float NextDelay()
        {
            if (_firstCall)
            {
                _firstCall = false;
                _currentDelay = _baseDelay;
                return _currentDelay;
            }

            _currentDelay = Math.Min(_currentDelay * _multiplier, _maxDelay);
            return _currentDelay;
        }

        public void Reset()
        {
            _firstCall = true;
            _currentDelay = _baseDelay;
        }
    }
}
