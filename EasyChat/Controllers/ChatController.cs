using Microsoft.AspNetCore.Mvc;
using EasyChat.Context;
using EasyChat.Models;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/Chat")]
public class ChatController : Controller
{
    public readonly DatabaseContext _db;
    public ChatController(DatabaseContext dbContext)
    {
        _db = dbContext;
    }

    [HttpGet("Create")]
    public async Task<IActionResult> CreateChatRoom()
    {
        
        Room room = new Room
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Unnamed Chat",
            IsKeepAlive = false
        };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        return Ok(room.Id);
        
    }

    [HttpGet("GetMessages/{roomId}")]
    public async Task<IActionResult> GetMessages(string roomId)
    {
        Message[] messages = await _db.Messages.Where(m => m.RoomId == roomId).ToArrayAsync();
        return Ok(messages);
    }
}