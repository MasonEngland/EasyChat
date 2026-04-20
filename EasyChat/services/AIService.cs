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
        try
        {
            using HttpClient client = new HttpClient();
            {
                client.Timeout = TimeSpan.FromMinutes(2);
                var response = await client.PostAsJsonAsync("http://ollama:11434/api/chat", new
                {
                    model ="mistral",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a helpful assistant in a chat room to help answer questions about general knowledge and trivia. Always be polite and formate responses in markdown. Keep responses brief. Use a conversational tone" },
                        new { role = "user", content = userMessage }
                    },
                    stream = false
                });


                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"OpenAI API error: {response.StatusCode}");

                    return "Sorry, I'm having trouble generating a response right now.";
                }

                var formattedResponse = await JsonSerializer.DeserializeAsync<AIRequest>(await response.Content.ReadAsStreamAsync());
                

                

                if (formattedResponse == null || formattedResponse.message == null || string.IsNullOrEmpty(formattedResponse.message.content))
                {
                    _logger.LogError("OpenAI API error: Invalid response format");
                    return "Sorry, I couldn't understand the response from the AI.";
                }

                return formattedResponse.message.content;
            };
        } catch (Exception ex)
        {
            Console.WriteLine($"Error calling OpenAI API: {ex.Message}");
            _logger.LogError(ex, "Error calling OpenAI API");
            return "Sorry, I'm having trouble generating a response right now.";
        }
        
    }
}