using Microsoft.AspNetCore.Mvc;
using EasyChat.Services;
using EasyChat.DTO;
using EasyChat.Context;
using EasyChat.Models;
using EasyChat.Hubs;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("Api/AI")]
public class AIController : ControllerBase
{
    public readonly IAIService _aiService;
    public readonly DatabaseContext _db;
    private readonly IHubContext<ChatHub> _hubContext;


    public AIController(IAIService aiService, DatabaseContext dbContext, IHubContext<ChatHub> hubContext)
    {
        _aiService = aiService;
        _db = dbContext;
        _hubContext = hubContext;
    }

    [HttpPost("GetResponse")]
    public async Task<IActionResult> GetResponse([FromBody] AIRequestDTO request)
    {
        Room? room = await _db.Rooms.FindAsync(request.RoomId);

        if (room == null) return NotFound("Room not found");

        var response = await _aiService.GetAIResponse(request.RoomId, request.UserMessage);

        await _hubContext.Clients.Group(request.RoomId).SendAsync("ReceiveMessage", "AI Assistant", response);

        return Ok(response);
    }
}