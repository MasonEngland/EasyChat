
using Microsoft.AspNetCore.Mvc;

namespace EasyChat.Controllers;




public class FileController : Controller
{
    [HttpPost("api/File/Upload/{RoomId}")]
    public async Task<IActionResult> UploadFile(string RoomId, IFormFile file)
    {
        return Ok();
    }

    [HttpGet("api/File/{RoomId}")]
    public async Task<IActionResult> GetFilesForRoom(string RoomId)
    {
        return Ok();
    }

    [HttpGet("api/File/Download/{FileId}")]
    public async Task<IActionResult> DownloadFile(string FileId)
    {
        return Ok();
    }

}