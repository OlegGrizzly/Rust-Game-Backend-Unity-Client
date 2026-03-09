using System;
using System.Linq;
using UnityEngine;
using GameBackend.Api;
using GameBackend.Core.Models;

namespace GameBackend.Samples
{
    /// <summary>
    /// Chat Example -- manual testing of Chat REST API operations.
    /// 1. First authenticate via AuthExample (tokens saved to PlayerPrefs)
    /// 2. Right-click -> Restore Session
    /// 3. Right-click -> List Channels / Create Channel / List Messages / etc.
    /// </summary>
    public class ChatExample : MonoBehaviour
    {
        [SerializeField] private string scheme = "http";
        [SerializeField] private string host = "localhost";
        [SerializeField] private int port = 30080;
        [SerializeField] private string channelId = "";
        [SerializeField] private string targetUserId = "";
        [SerializeField] private string channelName = "test-channel";
        [SerializeField] private string channelType = "direct";
        [SerializeField] private int messageLimit = 25;
        [TextArea(3, 10)]
        [SerializeField] private string lastResult = "";

        private GameClient _client;
        private GameClient Client => _client ?? (_client = new GameClient(scheme, host, port));

        [ContextMenu("Restore Session")]
        private void RestoreSession()
        {
            var restored = Client.RestoreSession();
            lastResult = restored
                ? $"Session restored: {Client.Session.UserId}"
                : "No saved session found";
            Debug.Log(lastResult);
        }

        [ContextMenu("List Channels")]
        private async void ListChannels()
        {
            try
            {
                var channels = await Client.ListChannelsAsync();
                var list = channels.ToList();
                lastResult = $"Channels ({list.Count}):";
                foreach (var ch in list)
                    lastResult += $"\n  [{ch.ChannelType}] {ch.Id} - {ch.Name}";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }

        [ContextMenu("Create Channel")]
        private async void CreateChannel()
        {
            try
            {
                var memberIds = string.IsNullOrEmpty(targetUserId)
                    ? Array.Empty<string>()
                    : new[] { targetUserId };

                var channel = await Client.CreateChannelAsync(new CreateChannelRequest
                {
                    Name = channelName,
                    ChannelType = channelType,
                    MemberIds = memberIds
                });

                channelId = channel.Id;
                lastResult = $"Created channel: {channel.Id}\nName: {channel.Name}\nType: {channel.ChannelType}";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }

        [ContextMenu("List Messages")]
        private async void ListMessages()
        {
            try
            {
                if (string.IsNullOrEmpty(channelId))
                {
                    lastResult = "channelId is empty. Set it in Inspector or create a channel first.";
                    Debug.LogWarning(lastResult);
                    return;
                }

                var result = await Client.ListMessagesAsync(channelId, limit: messageLimit);
                var messages = result.Messages;
                lastResult = $"Messages ({messages.Count}, has_more: {result.HasMore}):";
                foreach (var m in messages)
                    lastResult += $"\n  [{m.CreatedAt:HH:mm:ss}] {m.SenderName}: {m.Content}";
                if (!string.IsNullOrEmpty(result.NextCursor))
                    lastResult += $"\n  next_cursor: {result.NextCursor}";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }

        [ContextMenu("Get Unread")]
        private async void GetUnread()
        {
            try
            {
                if (string.IsNullOrEmpty(channelId))
                {
                    lastResult = "channelId is empty. Set it in Inspector.";
                    Debug.LogWarning(lastResult);
                    return;
                }

                var unread = await Client.GetUnreadAsync(channelId);
                lastResult = $"Unread count for {channelId}: {unread.UnreadCount}";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }

        [ContextMenu("Mark Read")]
        private async void MarkRead()
        {
            try
            {
                if (string.IsNullOrEmpty(channelId))
                {
                    lastResult = "channelId is empty. Set it in Inspector.";
                    Debug.LogWarning(lastResult);
                    return;
                }

                await Client.MarkChannelReadAsync(channelId);
                lastResult = $"Marked {channelId} as read";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }
    }
}
