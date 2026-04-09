namespace EasyChat.DTO;


public class UploadFileRequest
{
    public required string RoomId { get; set; }
    public required IFormFile File { get; set; }
    public required string User { get; set; }

}