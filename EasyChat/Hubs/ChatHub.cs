using Microsoft.AspNetCore.SignalR;


namespace EasyChat.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.Others.SendAsync("RecieveMessage", user, message);
    }   
}