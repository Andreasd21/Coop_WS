using Google.Cloud.PubSub.V1;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace CooP_WS.Hubs
{
    public class CanvasHub : Hub
    {
        private readonly PublisherClient _publisherClient;
        private const string TopicId = "Changes"; 

        public CanvasHub()
        {
            var projectId = "coop-443623"; 
            var topicName = TopicName.FromProjectTopic(projectId, TopicId);

            _publisherClient = PublisherClient.Create(topicName);
        }

        // Broadcast pixel updates to all connected clients and publish to Pub/Sub
        public async Task UpdatePixel(int x, int y, string color)
        {
                var messagePayload = new
            {
                x = x,
                y = y,
                color = color
            };
            var messageJson = JsonConvert.SerializeObject(messagePayload);

            await _publisherClient.PublishAsync(messageJson);

            // Send the message to other connected clients
            await Clients.Others.SendAsync("ReceivePixelUpdate", x, y, color);
        }
    }
}
