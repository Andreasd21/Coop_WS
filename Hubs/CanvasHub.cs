using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StackExchange.Redis;
using Google.Cloud.PubSub.V1;
using System.Threading.Tasks;

namespace CooP_WS.Hubs
{
    public class CanvasHub : Hub
    {
        private readonly PublisherClient _publisherClient;
        private readonly ISubscriber _redisSubscriber;
        private const string TopicId = "Changes";

        public CanvasHub(IConnectionMultiplexer redis)
        {
            var projectId = "coop-443623";
            var topicName = TopicName.FromProjectTopic(projectId, TopicId);

            _publisherClient = PublisherClient.Create(topicName);
            _redisSubscriber = redis.GetSubscriber();
        }

        public async Task UpdatePixel(int x, int y, string color)
        {
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
