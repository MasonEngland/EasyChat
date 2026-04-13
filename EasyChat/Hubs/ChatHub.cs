using Microsoft.AspNetCore.SignalR;
using EasyChat.Context;
using EasyChat.Models;
using EasyChat.Services;


namespace EasyChat.Hubs;

public class ChatHub : Hub
{
    private readonly DatabaseContext _db;
    private readonly IMessageService _messageService;

    public ChatHub(DatabaseContext dbContext, IMessageService messageService)
    {
        _db = dbContext;
        _messageService = messageService;
    }

    public async Task JoinRoom(string user, string roomId)
    {
        Context.Items["roomId"] = roomId;
        // if room doesn't exist, create it
        Room? room = await _db.Rooms.FindAsync(roomId);

       
        if (room == null)
        {
            await Clients.Caller.SendAsync("CatchError", "Room not found. Please refresh the page and join a room again.");
            return;
        }

        room.lastActive = DateTime.UtcNow;
        await _db.SaveChangesAsync();


        await Groups.AddToGroupAsync(Context.ConnectionId, room.Id);

        await Clients.Group(room.Id).SendAsync("ReceiveMessage", "EasyChat", $"{user} joined.");
    }

    public async Task SendMessage(string user, string message)
    {
        Console.WriteLine($"Received message from {user}: {message}");
        string? roomId = Context.Items["roomId"] as string;
        if (roomId == null || roomId == "")
        {
            Console.WriteLine("Error: roomId is null or empty.");
            await Clients.Caller.SendAsync("CatchError", "Session Expired. Please refresh the page and join a room again.");
            return;
        }

        await _messageService.SaveMessage(roomId, user, message);

        await Clients.OthersInGroup(roomId).SendAsync("ReceiveMessage", user, message);
    }  
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? roomId = Context.Items["roomId"] as string;

        if (roomId == null || roomId == "")
        {
            return;
        }

        await Clients.OthersInGroup(roomId).SendAsync("ReceiveMessage", "EasyChat", $"A user has left the chat.");
        if (roomId != null && roomId != "")
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    // public async Task SendFile(string user, string fileName)
    // {
    //     string? roomId = Context.Items["roomId"] as string;

    //     if (roomId == null || roomId == "")
    //     {
    //         Console.WriteLine("Error: roomId is null or empty.");
    //         await Clients.Caller.SendAsync("CatchError", "Session Expired. Please refresh the page and join a room again.");
    //         return;
    //     }
    //     await _messageService.SaveMessage(roomId, user, fileName);


    //     await Clients.OthersInGroup(roomId).SendAsync("ReceiveFile", user, fileName);
    // }
}