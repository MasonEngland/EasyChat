using Microsoft.AspNetCore.Mvc;
using EasyChat.Context;
using EasyChat.Models;
using EasyChat.Services;
using Microsoft.EntityFrameworkCore;

namespace EasyChat.Controllers;


[ApiController]
[Route("Api/Chat")]
public class ChatController : ControllerBase
{
    public readonly DatabaseContext _db;
    private readonly IMessageService _messageService;
    public ChatController(DatabaseContext dbContext, IMessageService messageService)
    {
        _db = dbContext;
        _messageService = messageService;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateChatRoom()
    {
        return Ok(await _messageService.CreateRoom());
        
    }

    [HttpGet("GetMessages/{roomId}")]
    public async Task<IActionResult> GetMessages(string roomId)
    {
        if (roomId == null || roomId == "")
        {
            return BadRequest("Invalid room ID.");
        }
        if (!await _db.Rooms.AnyAsync(r => r.Id == roomId))
        {
            return NotFound("Room not found.");
        }
        Message[] messages = await _messageService.GetMessagesForRoom(roomId);
        return Ok(messages);
    }

    [HttpPost("UpdateRoomLife/{keepAlive}")]
    public async Task<IActionResult> UpdateRoomLife(string keepAlive)
    {
        bool keepAliveValue = keepAlive == "true" ? true : false;
        await _db.Rooms.Where(r => r.IsKeepAlive == keepAliveValue).ForEachAsync(r => r.lastActive = DateTime.UtcNow);
        return Ok();
        
    }
}