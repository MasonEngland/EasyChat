namespace EasyChat.Models;

public class FileMessage
{
    public int Id { get; set; }
    public required string FileName { get; set; }
    public required string FilePath { get; set; }
    public required string RoomId { get; set; }

    public required string User { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Room? Room { get; set; }

    public FileMessage()
    {
        Timestamp = DateTime.UtcNow;
    }
}