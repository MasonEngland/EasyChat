namespace EasyChat.Models;


public class Message
{
    public int Id { get; set; }
    public required string RoomId { get; set; }
    public required string User { get; set; }
    public required string Text { get; set; }

    public required Room Room { get; set; }

    public DateTime Timestamp { get; set; }

    public Message()
    {
        Timestamp = DateTime.UtcNow;
    }
}