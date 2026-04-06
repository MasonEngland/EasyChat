using EasyChat.Models;
using EasyChat.Context;
using Microsoft.EntityFrameworkCore;

namespace EasyChat.Services;


public class MessageService(DatabaseContext dbContext) : IMessageService
{
    private readonly DatabaseContext _db = dbContext;

    public async Task<string> CreateRoom()
    {
        Room room = new Room
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Unnamed Chat",
            IsKeepAlive = false
        };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        return room.Id;
    }

    public async Task<Message[]> GetMessagesForRoom(string roomId)
    {
        return await _db.Messages.Where(m => m.RoomId == roomId).ToArrayAsync();
    }

    public async Task SaveMessage(string roomId, string user, string text)
    {
        try
        {
            Message message = new Message
            {
                RoomId = roomId,
                User = user,
                Text = text
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving message: {ex.Message}");
            throw;
        }
    }
}