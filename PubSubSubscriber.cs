using CooP_WS.Hubs;
using Google.Cloud.PubSub.V1;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CooP_WS
{
    public class PubSubSubscriber
    {
        private readonly SubscriberClient _subscriberClient;
        private readonly IHubContext<CanvasHub> _hubContext;

        public PubSubSubscriber(string projectId, string subscriptionId, IHubContext<CanvasHub> hubContext)
        {
            var subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
            _subscriberClient = SubscriberClient.Create(subscriptionName);
            _hubContext = hubContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _subscriberClient.StartAsync(async (PubsubMessage message, CancellationToken token) =>
            {
                try
                {
                    // Decode the Pub/Sub message
                    var messageData = System.Text.Encoding.UTF8.GetString(message.Data.ToByteArray());
                    var pixelUpdate = JsonConvert.DeserializeObject<dynamic>(messageData);

                    // Broadcast the message to all connected clients
                    await _hubContext.Clients.All.SendAsync("ReceivePixelUpdate",
                        (int)pixelUpdate.x,
                        (int)pixelUpdate.y,
                        (string)pixelUpdate.color);

                    Console.WriteLine($"Message received and broadcasted: {messageData}");

                    // Acknowledge the message
                    return SubscriberClient.Reply.Ack;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    return SubscriberClient.Reply.Nack;
                }
            });
        }
    }
}
