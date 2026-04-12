using EasyChat.DTO;
using EasyChat.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using EasyChat.Hubs;

namespace EasyChat.Controllers;


[ApiController]
[Route("Api/File")]
public class FileController : ControllerBase
{
    
    private readonly IFileService _fileService;
    private readonly IHubContext<ChatHub> _hubContext;
    public FileController(IFileService fs, IHubContext<ChatHub> hubContext)
    {
        _fileService = fs;
        _hubContext = hubContext;
    }

    [HttpPost("Upload")]
    public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
    {
        bool success = await _fileService.UploadFile(request.RoomId, request.File, request.User);

        if (!success) return BadRequest("File upload failed. Please try again.");

        await _hubContext.Clients.Group(request.RoomId).SendAsync("ReceiveFile", request.User, request.File.FileName);

        return Ok("File uploaded successfully.");
    }

    [HttpGet("{RoomId}")]
    public async Task<IActionResult> GetFilesForRoom(string RoomId)
    {
        var files = await _fileService.GetFilesForRoom(RoomId);

        if (files == null || files.Length == 0)
        {
            return NotFound("No files found for this room.");
        }

        return Ok(files);
    }

    [HttpGet("Download/{FileId}")]
    public async Task<IActionResult> DownloadFile(string FileId)
    {
        try
        {
            string? filePath = await _fileService.GetFilePath(FileId);
            if (filePath == null)
            {
                return NotFound("File not found.");
            }

            string fileName = Path.GetFileName(filePath);
            return PhysicalFile(filePath, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading file: {ex.Message}");
            return NotFound("an error occurred while trying to download the file");
        }
    }

}