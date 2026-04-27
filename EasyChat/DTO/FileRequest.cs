namespace EasyChat.DTO;


public class UploadFileRequest
{
    public required string RoomId { get; set; }
    public required IFormFile File { get; set; }
    public required string User { get; set; }


}

public class AIRequest
{
    public required string model { get; set;}
    public required AIMessage message { get; set; }
}

public class AIRequestDTO
{
    public required string RoomId { get; set; }
    public required string UserMessage { get; set; }
}


public class AIMessage
{
    public required string role { get; set; }
    public required string content { get; set; }
}

public class KeepAliveRequest
{
    public required bool KeepAlive { get; set; }
    public required string RoomId { get; set; }
}