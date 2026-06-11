using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Database.Entities;
using Database.Dto;
using Database;
using System.Diagnostics;
using AutoMapper;

namespace WebApplication1.Services
{
    public class SSEService : IService
    {
        private ConcurrentDictionary<string, HttpResponse> _clients = new();

        public void AddClient(int userId, HttpResponse response)
        {
            Debug.Write("UserId for notifs " + userId);
            _clients[userId.ToString()] = response;
        }

        public void RemoveClient(int userId)
        {
            _clients.TryRemove(userId.ToString(), out _);
        }

        public async Task getUnread(int userId, DatabaseContext context, IMapper maper, CancellationToken token)
        {

            var notifs = context.Notifications.Where(x => x.UserId == userId && !x.Read).ToList();

            if (notifs.Any())
            {

                var notifsDto = new List<NotificationDto>();
                foreach (var item in notifs)
                {
                    notifsDto.Add(maper.Map<NotificationDto>(item));
                }
                if (_clients.TryGetValue(userId.ToString(), out var response) && !token.IsCancellationRequested)
                {
                    Debug.WriteLine($"Sending {notifsDto.Count} notifications to user {userId}");
                    var json = JsonSerializer.Serialize(notifsDto);
                    await response.WriteAsync($"event: notification\n");
                    await response.WriteAsync($"data: {json}\n\n", token);
                    await response.Body.FlushAsync(token);
                }
            }
        }


        public async Task SendToUserAsync(NotificationDto not, CancellationToken token)
        {
            if (_clients.TryGetValue(not.UserId.ToString(), out var response) && !token.IsCancellationRequested)
            {
                Debug.WriteLine($"Sending notification to use {not.UserId}");
                var json = JsonSerializer.Serialize(not);
                await response.WriteAsync($"event: notification\n");
                await response.WriteAsync($"data: {json}\n\n", token);
                await response.Body.FlushAsync(token);
            }
        }

        public async Task BroadcastAsync(object message, CancellationToken token)
        {
            var json = JsonSerializer.Serialize(message);
            foreach (var client in _clients.Values)
            {
                if (!token.IsCancellationRequested)
                {

                    await client.WriteAsync($"data: {json}\n\n", token);
                    await client.Body.FlushAsync(token);
                }
            }
        }
    }
}
