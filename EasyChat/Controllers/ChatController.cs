using Microsoft.AspNetCore.Mvc;
using EasyChat.Context;
using EasyChat.Models;
using EasyChat.Services;
using Microsoft.EntityFrameworkCore;
using EasyChat.DTO;
using Microsoft.AspNetCore.Http.HttpResults;

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

    [HttpPost("UpdateRoomLife")]
    public async Task<IActionResult> UpdateRoomLife([FromBody] KeepAliveRequest request)
    {
        bool keepAliveValue = request.KeepAlive == true;

        try
        {
            await _db.Rooms.Where(r => r.Id == request.RoomId).ExecuteUpdateAsync(s => s.SetProperty(r => r.IsKeepAlive, keepAliveValue));
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while updating the room: {ex.Message}");
        }
    }

    // added this for checkbox persistence on client side
    [HttpGet("KeepAlive/{roomId}")]
    public async Task<IActionResult> GetKeepAlive(string roomId)
    {
        var room = await _db.Rooms.FindAsync(roomId);
        if (room == null) return NotFound();
        return Ok(room.IsKeepAlive);
    }
}