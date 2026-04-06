using EasyChat.Hubs;
using Microsoft.EntityFrameworkCore;
using EasyChat.Context;
using EasyChat.Services;

namespace EasyChat;

public class EasyChat 
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSignalR();
        builder.Services.AddControllers();
        builder.Services.AddScoped<IMessageService, MessageService>();
        builder.Services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseSqlite("Data Source=database.db;Foreign Keys=True");
        });

        var app = builder.Build();

        app.MapControllers();
        app.UseStaticFiles();
        app.MapFallbackToFile("index.html");
        app.MapHub<ChatHub>("/Chat");

        app.Run();

    }
}



