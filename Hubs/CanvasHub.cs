using Google.Cloud.PubSub.V1;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

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

        // Publish to Pub/Sub
        public async Task UpdatePixel(int x, int y, string color)
        {
            var messagePayload = new
            {
                x,
                y,
                color
            };
            var messageJson = JsonConvert.SerializeObject(messagePayload);

            // Publish the message to Pub/Sub
            await _publisherClient.PublishAsync(messageJson);

            // Optionally log or acknowledge the message
            Console.WriteLine("Message published to Pub/Sub.");
        }
    }
}
