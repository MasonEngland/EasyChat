using Microsoft.AspNetCore.Mvc;
using EasyChat.Services;
using EasyChat.DTO;
using EasyChat.Hubs;
using Microsoft.AspNetCore.SignalR;


[ApiController]
[Route("Api/Streaming")]
public class StreamingController : ControllerBase
{

    private readonly IRoomVideoPathsDictionary<string, string> _roomVideoPaths;
    private readonly IHubContext<ChatHub> _hubContext;
    public StreamingController(IRoomVideoPathsDictionary<string, string> roomVideoPaths, IHubContext<ChatHub> hubContext)
    {
        _roomVideoPaths = roomVideoPaths;
        _hubContext = hubContext;
    }

    [HttpPost("UploadVideo")]
    [RequestSizeLimit(524_288_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)]

    public async Task<IActionResult> UploadVideo([FromForm] UploadFileRequest request)
    {
        try
        {
            if (request.File.ContentType != "video/mp4")
            {
                return BadRequest("Only MP4 video files are allowed.");
            }

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", request.RoomId);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string filePath = Path.Combine(uploadsFolder, $"{Guid.NewGuid()}_{request.File.FileName}");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            };

            
            _roomVideoPaths[request.RoomId] = filePath;

            await _hubContext.Clients.Group(request.RoomId).SendAsync("ReceiveVideo", request.User);
            return Ok(new { Success = true, Path = filePath });
        } catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
        
    }

    [HttpGet("Watch/{RoomId}")]
    public IActionResult WatchVideo(string RoomId)
    {
        if (!_roomVideoPaths.TryGetValue(RoomId, out string? filePath))
        {
            return NotFound("No video found for this room.");
        }

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("No video found for this room.");
        }


        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(stream, "video/mp4", enableRangeProcessing: true);
    }
}