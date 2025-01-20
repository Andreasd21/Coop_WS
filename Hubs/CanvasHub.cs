using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StackExchange.Redis;
using Google.Cloud.PubSub.V1;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CooP_WS.Hubs
{
    public class CanvasHub : Hub
    {
        private readonly PublisherClient _publisherClient;
        private readonly ISubscriber _redisSubscriber;
        private const string TopicId = "Changes";
        private static readonly ConcurrentDictionary<string, (DateTime LastMessageTime, int MessageCount)> MessageRates = new();

        public CanvasHub(IConnectionMultiplexer redis)
        {
            var projectId = "coop-443623";
            var topicName = TopicName.FromProjectTopic(projectId, TopicId);

            _publisherClient = PublisherClient.Create(topicName);
            _redisSubscriber = redis.GetSubscriber();
        }

        public async Task UpdatePixel(int x, int y, string color)
        {
            var clientId = Context.ConnectionId;

            // Rate limiting logic
            if (MessageRates.TryGetValue(clientId, out var rate))
            {
                var timeSinceLastMessage = DateTime.UtcNow - rate.LastMessageTime;

                // Reset the message count if the time window has passed
                if (timeSinceLastMessage > TimeSpan.FromSeconds(1))
                {
                    MessageRates[clientId] = (DateTime.UtcNow, 1);
                }
                else
                {
                    if (rate.MessageCount >= 5) // Allow 5 messages per second
                    {
                        Console.WriteLine($"Rate limit exceeded for client: {clientId}");
                        Context.Abort(); // Disconnect the client if they exceed the rate limit
                        return;
                    }

                    MessageRates[clientId] = (rate.LastMessageTime, rate.MessageCount + 1);
                }
            }
            else
            {
                // First message from this client
                MessageRates[clientId] = (DateTime.UtcNow, 1);
            }

            // Proceed with pixel update
            var messagePayload = new
            {
                x = x,
                y = y,
                color = color
            };

            // Serialize the message payload
            var messageJson = JsonConvert.SerializeObject(messagePayload);

            // Publish to Google Pub/Sub
            await _publisherClient.PublishAsync(messageJson);

            // Publish to Redis for SignalR clients
            await _redisSubscriber.PublishAsync("PixelUpdates", messageJson);

            Console.WriteLine($"Pixel updated: x={x}, y={y}, color={color}");
        }
    }
}
