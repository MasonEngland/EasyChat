using Microsoft.AspNetCore.SignalR;


namespace EasyChat.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        Console.WriteLine($"Received message from {user}: {message}");
        await Clients.All.SendAsync("RecieveMessage", user, message);
    }   
}