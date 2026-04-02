namespace EasyChat.Models;



public class Room
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool IsKeepAlive { get; set; }
    public required DateTime lastActive { get; set; }
    public List<Message> Messages { get; set; } = new List<Message>();


    public Room()
    {
        Id = Guid.NewGuid().ToString();
        lastActive = DateTime.UtcNow;
    }
}