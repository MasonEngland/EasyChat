using EasyChat.DTO;
using EasyChat.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasyChat.Controllers;


[ApiController]
[Route("Api/File")]
public class FileController : ControllerBase
{
    
    private readonly IFileService _fileService;
    public FileController(IFileService fs)
    {
        _fileService = fs;
    }

    [HttpPost("Upload")]
    public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
    {
        bool success = await _fileService.UploadFile(request.RoomId, request.File, request.User);

        if (!success) return BadRequest("File upload failed. Please try again.");
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
            return NotFound("an error occurred while trying to download the file");
        }
    }

}