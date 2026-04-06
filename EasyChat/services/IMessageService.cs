using EasyChat.Models;

namespace EasyChat.Services;

public interface IMessageService
{
    Task<string> CreateRoom();
    Task<Message[]> GetMessagesForRoom(string roomId);
    Task SaveMessage(string roomId, string user, string text);
}