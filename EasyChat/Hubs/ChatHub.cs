using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.Connections; 


namespace EasyChat.Hubs;

public class ChatHub : Hub
{
    private string roomId => Context.GetHttpContext()?.Request.Query["roomId"]!;

    public async Task JoinRoom(string user)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        await Clients.Group(roomId).SendAsync("RecieveMessage", "System", $"{user} joined.");
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.OthersInGroup(roomId).SendAsync("RecieveMessage", user, message);
    }   
}