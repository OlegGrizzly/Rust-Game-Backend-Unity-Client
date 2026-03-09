using System;
using UnityEngine;
using GameBackend.Api;
using GameBackend.Core.Interfaces;

namespace GameBackend.Samples
{
    /// <summary>
    /// Example MonoBehaviour demonstrating all IAuthClient methods.
    ///
    /// Testing instructions:
    /// 1. Attach this script to a GameObject in a scene
    /// 2. Configure host/port/scheme in the Inspector
    /// 3. Right-click on the component header and select a [ContextMenu] action
    /// 4. Check lastResult field in Inspector or Console for output
    /// </summary>
    public class AuthExample : MonoBehaviour
    {
        [SerializeField] private string scheme = "http";
        [SerializeField] private string host = "localhost";
        [SerializeField] private int port = 30080;
        [SerializeField] private string testUsername = "testplayer";
        [SerializeField] private string testPassword = "TestPass123!";
        [SerializeField] private string testEmail = "test@example.com";
        [SerializeField] private string testDeviceId = "550e8400-e29b-41d4-a716-446655440000";
        [TextArea(3, 10)]
        [SerializeField] private string lastResult = "";

        private GameClient _client;

        private GameClient Client
        {
            get
            {
                if (_client == null)
                    _client = new GameClient(scheme, host, port);
                return _client;
            }
        }

        [ContextMenu("Register Username")]
        private async void RegisterUsername()
        {
            try
            {
                var session = await Client.AuthenticateUsernameAsync(testUsername, testPassword);
                lastResult = $"Registered: {session.UserId} ({session.Username})";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("Register Email")]
        private async void RegisterEmail()
        {
            try
            {
                var session = await Client.AuthenticateEmailAsync(testEmail, testPassword, testUsername);
                lastResult = $"Registered: {session.UserId} ({session.Username})";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("Register Device")]
        private async void RegisterDevice()
        {
            try
            {
                var session = await Client.AuthenticateDeviceAsync(testDeviceId);
                lastResult = $"Registered: {session.UserId}";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("Login Username")]
        private async void LoginUsername()
        {
            try
            {
                var session = await Client.LoginUsernameAsync(testUsername, testPassword);
                lastResult = $"Logged in: {session.UserId} ({session.Username})";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("Login Email")]
        private async void LoginEmail()
        {
            try
            {
                var session = await Client.LoginEmailAsync(testEmail, testPassword);
                lastResult = $"Logged in: {session.UserId} ({session.Username})";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("Login Device")]
        private async void LoginDevice()
        {
            try
            {
                var session = await Client.LoginDeviceAsync(testDeviceId);
                lastResult = $"Logged in: {session.UserId}";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("Refresh Session")]
        private async void RefreshSession()
        {
            try
            {
                var session = await Client.RefreshSessionAsync();
                lastResult = $"Refreshed: {session.UserId}";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("Logout")]
        private async void Logout()
        {
            try
            {
                await Client.LogoutAsync();
                lastResult = "Logged out successfully";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("List Sessions")]
        private async void ListSessions()
        {
            try
            {
                var sessions = await Client.ListSessionsAsync();
                lastResult = $"Sessions count: {sessions.Count}";
                foreach (var s in sessions)
                    lastResult += $"\n  {s.Id} ({s.DeviceInfo})";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("Revoke All Sessions")]
        private async void RevokeAllSessions()
        {
            try
            {
                await Client.RevokeAllSessionsAsync();
                lastResult = "All sessions revoked";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("Restore Session")]
        private void RestoreSession()
        {
            var restored = Client.RestoreSession();
            lastResult = restored
                ? $"Session restored: {Client.Session.UserId}"
                : "No saved session found";
            Debug.Log(lastResult);
        }

        [ContextMenu("Clear Session")]
        private void ClearSession()
        {
            Client.ClearSession();
            lastResult = "Session cleared";
            Debug.Log(lastResult);
        }

        [ContextMenu("Check Is Authenticated")]
        private void CheckAuthenticated()
        {
            lastResult = $"IsAuthenticated: {Client.IsAuthenticated}";
            if (Client.Session != null)
                lastResult += $"\nUserId: {Client.Session.UserId}, IsExpired: {Client.Session.IsExpired}";
            Debug.Log(lastResult);
        }
    }
}
