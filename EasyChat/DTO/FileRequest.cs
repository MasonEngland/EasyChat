namespace EasyChat.DTO;


public class UploadFileRequest
{
    public required string RoomId { get; set; }
    public required IFormFile File { get; set; }
    public required string User { get; set; }


}

public class AIRequest
{
    public required string Model { get; set;}
    public required AIMessage Message { get; set; }
}

public class AIRequestDTO
{
    public required string RoomId { get; set; }
    public required string UserMessage { get; set; }
}


public class AIMessage
{
    public required string Role { get; set; }
    public required string Content { get; set; }
}