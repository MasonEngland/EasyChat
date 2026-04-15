using EasyChat.Context;
using System.Text.Json;
using EasyChat.DTO;

namespace EasyChat.Services;

public class AIService : IAIService
{
    private readonly ILogger<AIService> _logger;

    public AIService(ILogger<AIService> logger)
    {
        _logger = logger;

    }

    public async Task<string> GetAIResponse(string roomId, string userMessage)
    {
        using HttpClient client = new HttpClient();
        {
            var response = await client.PostAsJsonAsync("http://localhost:11434/api/generate", new
            {
                model ="mistral",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant in a chat room to help answer questions about general knowledge and trivia. Always be polite and formate responses in markdown. Keep responses brief. Use a conversational tone" },
                    new { role = "user", content = userMessage }
                },
                stream = false
            });

            var formattedResponse = await JsonSerializer.DeserializeAsync<AIRequest>(await response.Content.ReadAsStreamAsync());
            

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"OpenAI API error: {response.StatusCode}");

                return "Sorry, I'm having trouble generating a response right now.";
            }

            if (formattedResponse == null || formattedResponse.Message == null || string.IsNullOrEmpty(formattedResponse.Message.Content))
            {
                _logger.LogError("OpenAI API error: Invalid response format");
                return "Sorry, I couldn't understand the response from the AI.";
            }

            return formattedResponse.Message.Content;
        };
    }
}