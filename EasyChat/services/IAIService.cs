namespace EasyChat.Services;

public interface IAIService
{
    Task<string> GetAIResponse(string roomId, string userMessage);
}