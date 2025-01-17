using Google.Api;
using Google.Apis.Auth;
using Google.Cloud.PubSub.V1;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Google.Apis.Auth;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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


public override async Task OnConnectedAsync()
    {
        try
        {
            var httpContext = Context.GetHttpContext();
            var authorizationHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (authorizationHeader == null || !authorizationHeader.StartsWith("Bearer "))
            {
                throw new UnauthorizedAccessException("No token provided.");
            }

            var token = authorizationHeader.Substring("Bearer ".Length);

            // Validate the token using Google.Apis.Auth
            var payload = await GoogleJsonWebSignature.ValidateAsync(token);
            Console.WriteLine($"Token valid. User ID: {payload.Subject}");

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication failed: {ex.Message}");
            Context.Abort(); // Disconnect the client
        }
    }

    // Broadcast pixel updates to all connected clients and publish to Pub/Sub
    public async Task UpdatePixel(int x, int y, string color)
        {
            var messagePayload = new
            {
                x,
                y,
                color
            };
            var messageJson = JsonConvert.SerializeObject(messagePayload);

            await _publisherClient.PublishAsync(messageJson);

            // Send the message to other connected clients
            await Clients.Others.SendAsync("ReceivePixelUpdate", x, y, color);
        }
    }
}
