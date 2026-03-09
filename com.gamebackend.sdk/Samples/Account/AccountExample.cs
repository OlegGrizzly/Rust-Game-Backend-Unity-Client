using System;
using System.Linq;
using UnityEngine;
using GameBackend.Api;

namespace GameBackend.Samples
{
    /// <summary>
    /// Account Example -- manual testing of profile operations.
    /// 1. First authenticate via AuthExample
    /// 2. Right-click -> Get My Account
    /// 3. Right-click -> Update Display Name
    /// </summary>
    public class AccountExample : MonoBehaviour
    {
        [SerializeField] private string scheme = "http";
        [SerializeField] private string host = "localhost";
        [SerializeField] private int port = 30080;
        [SerializeField] private string testDisplayName = "New Name";
        [SerializeField] private string testUserId = "";
        [TextArea(3, 10)]
        [SerializeField] private string lastResult = "";

        private GameClient _client;
        private GameClient Client => _client ?? (_client = new GameClient(scheme, host, port));

        [ContextMenu("Get My Account")]
        private async void GetMyAccount()
        {
            try
            {
                var account = await Client.GetAccountAsync();
                lastResult = $"Account: {account.Id}\nUsername: {account.Username}\nDisplay: {account.DisplayName}";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }

        [ContextMenu("Update Display Name")]
        private async void UpdateDisplayName()
        {
            try
            {
                await Client.UpdateAccountAsync(displayName: testDisplayName);
                lastResult = $"Updated display name to: {testDisplayName}";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }

        [ContextMenu("Delete Account")]
        private async void DeleteAccount()
        {
            try
            {
                await Client.DeleteAccountAsync();
                lastResult = "Account deleted";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }

        [ContextMenu("Get User By Id")]
        private async void GetUserById()
        {
            try
            {
                var user = await Client.GetUserAsync(testUserId);
                lastResult = $"User: {user.Id} ({user.Username}) - {user.DisplayName}";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }

        [ContextMenu("Get Users Batch")]
        private async void GetUsersBatch()
        {
            try
            {
                var users = await Client.GetUsersAsync(new[] { testUserId });
                var list = users.ToList();
                lastResult = $"Found {list.Count} users";
                foreach (var u in list) lastResult += $"\n  {u.Id} ({u.Username})";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }
    }
}
