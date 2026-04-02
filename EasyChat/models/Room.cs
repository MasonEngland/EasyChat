namespace EasyChat.Models;



public class Room
{
    public string Id { get; set; }
    public required string Name { get; set; }
    public required bool IsKeepAlive { get; set; }
    public DateTime lastActive { get; set; }
    public List<Message> Messages { get; set; } = [];


    public Room()
    {
        Id = Guid.NewGuid().ToString();
        lastActive = DateTime.UtcNow;
    }
}