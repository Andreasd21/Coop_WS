using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StackExchange.Redis;
using Google.Cloud.PubSub.V1;
using System.Threading.Tasks;
using CooP_WS.Modal;

namespace CooP_WS.Hubs
{
    public class CanvasHub : Hub
    {
        private readonly PublisherClient _publisherClient;
        private readonly ISubscriber _redisSubscriber;
        private readonly IDatabase _redisDatabase;
        private const string TopicId = "Changes";

        public CanvasHub(IConnectionMultiplexer redis)
        {
            var projectId = "coop-443623";
            var topicName = TopicName.FromProjectTopic(projectId, TopicId);

            _publisherClient = PublisherClient.Create(topicName);
            _redisSubscriber = redis.GetSubscriber();
            _redisDatabase = redis.GetDatabase();
        }

        public override async Task OnConnectedAsync()
        {
            // Subscribe to Redis updates when a client connects
            await _redisSubscriber.SubscribeAsync("PixelUpdates", async (channel, message) =>
            {
                var pixelUpdate = JsonConvert.DeserializeObject<PixelUpdate>(message);
                await Clients.All.SendAsync("ReceivePixelUpdate", pixelUpdate.X, pixelUpdate.Y, pixelUpdate.Color);
            });

            await base.OnConnectedAsync();
        }

        public async Task UpdatePixel(int x, int y, string color)
        {
            // Publish updates to Redis
            var pixelUpdate = new PixelUpdate { X = x, Y = y, Color = color };
            var message = JsonConvert.SerializeObject(pixelUpdate);

            await _redisSubscriber.PublishAsync("PixelUpdates", message);

            // Optionally store the update in Redis
            await _redisDatabase.StringSetAsync($"Pixel:{x}:{y}", color);

            System.Console.WriteLine($"Pixel updated: x={x}, y={y}, color={color}");
        }
    }
}
