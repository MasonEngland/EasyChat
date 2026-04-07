using EasyChat.Models;

namespace EasyChat.Services;

public static class ServiceExtensions
{
    public static void AddChatServices(this IServiceCollection services)
    {
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IFileService, FileService>();
    }
}