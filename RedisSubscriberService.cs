using CooP_WS.Hubs;
using CooP_WS.Modal;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace CooP_WS
{
    public class RedisSubscriberService
    {
        private readonly ISubscriber _redisSubscriber;
        private readonly IHubContext<CanvasHub> _hubContext;

        public RedisSubscriberService(IConnectionMultiplexer redis, IHubContext<CanvasHub> hubContext)
        {
            _redisSubscriber = redis.GetSubscriber();
            _hubContext = hubContext;

            SubscribeToRedisChannel();
        }

        private void SubscribeToRedisChannel()
        {
            _redisSubscriber.Subscribe("PixelUpdates", async (channel, message) =>
            {
                try
                {
                    var pixelUpdate = JsonConvert.DeserializeObject<PixelUpdate>(message);
                    Console.WriteLine($"Received message from Redis: {message}");

                    // Broadcast to all SignalR clients
                    await _hubContext.Clients.All.SendAsync("ReceivePixelUpdate", pixelUpdate.X, pixelUpdate.Y, pixelUpdate.Color);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing Redis message: {ex.Message}");
                }
            });
        }
    }
}

