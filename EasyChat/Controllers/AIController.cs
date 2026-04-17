using Microsoft.AspNetCore.Mvc;
using EasyChat.Services;
using EasyChat.DTO;

[ApiController]
[Route("Api/AI")]
public class AIController : ControllerBase
{
    public readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("GetResponse")]
    public async Task<IActionResult> GetResponse([FromBody] AIRequestDTO request)
    {
        var response = await _aiService.GetAIResponse(request.RoomId, request.UserMessage);
        return Ok(response);
    }
}