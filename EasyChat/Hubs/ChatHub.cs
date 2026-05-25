using Microsoft.AspNetCore.SignalR;
using EasyChat.Context;
using EasyChat.Models;
using EasyChat.Services;
using Microsoft.EntityFrameworkCore;


namespace EasyChat.Hubs;

public class ChatHub : Hub
{
    private readonly DatabaseContext _db;
    private readonly IMessageService _messageService;
    private readonly IRoomVideoPathsDictionary<string, string> _roomVideoPaths;

    public ChatHub(DatabaseContext dbContext, IMessageService messageService, IRoomVideoPathsDictionary<string, string> roomVideoPaths)
    {
        _db = dbContext;
        _messageService = messageService;
        _roomVideoPaths = roomVideoPaths;

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

        Room? room = await _db.Rooms.Where(r => r.Id == roomId).FirstOrDefaultAsync();
        room?.lastActive = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _messageService.SaveMessage(roomId, user, message);

        await Clients.OthersInGroup(roomId).SendAsync("ReceiveMessage", user, message);
    }  
    
    public async Task BroadcastStreamUpdate(string roomId, double timestamp, bool isPaused, bool isMuted, bool isStopped)
    {
        try
        {
            if (isStopped)
            {
                // get video path from _roomVideoPaths and delete the file
                if (_roomVideoPaths.TryGetValue(roomId, out string? filePath))
                {
                    Console.WriteLine($"Removing video for room {roomId} at path {filePath}");
                    _roomVideoPaths.Remove(roomId);
                    // Delete the video file from the file system
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }
            if (roomId == null || roomId == "")
            {
                Console.WriteLine("Error: roomId is null or empty.");
                await Clients.Caller.SendAsync("CatchError", "Session Expired. Please refresh the page and join a room again.");
                return;
            }

            await Clients.OthersInGroup(roomId).SendAsync("ReceiveStreamUpdate", timestamp, isPaused, isMuted, isStopped);
        } catch (Exception ex)
        {
            Console.WriteLine($"Error broadcasting stream update: {ex.Message}");
        }
        
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

}