using Microsoft.AspNetCore.SignalR;
using EasyChat.Context;
using EasyChat.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace EasyChat.Hubs;

public class ChatHub : Hub
{
    private readonly DatabaseContext _db;

    public ChatHub(DatabaseContext dbContext)
    {
        _db = dbContext;
    }

    public async Task JoinRoom(string user, string roomId)
    {
        Context.Items["roomId"] = roomId;
        // if room doesn't exist, create it
        Room? room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
        {
            await Clients.Caller.SendAsync("CatchError", "Room not found. Please refresh the page and join a room again.");
            return;
        }


        await Groups.AddToGroupAsync(Context.ConnectionId, room.Id);

        await Clients.Group(room.Id).SendAsync("ReceiveMessage", "System", $"{user} joined.");
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

        try
        {
            Message newMessage = new Message
            {
                RoomId = roomId,
                User = user,
                Text = message,
                Timestamp = DateTime.UtcNow
            };
            _db.Messages.Add(newMessage);

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving message: {ex.Message}");
            await Clients.Caller.SendAsync("CatchError", "Failed to send message. Please try again.");
            return;
        }

        await Clients.OthersInGroup(roomId).SendAsync("ReceiveMessage", user, message);
    }   
}