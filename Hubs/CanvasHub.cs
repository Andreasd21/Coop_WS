using Microsoft.AspNetCore.SignalR;

namespace CooP_WS.Hubs
{
    public class CanvasHub : Hub
    {
        // Broadcast pixel updates to all connected clients
        public async Task UpdatePixel(int x, int y, string color)
        {
            await Clients.Others.SendAsync("ReceivePixelUpdate", x, y, color);
        }
    }
}
