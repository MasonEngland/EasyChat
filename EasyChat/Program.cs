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
        builder.Services.AddChatServices();
        builder.Services.AddHostedService<RoomCleanupWorker>();
        builder.Services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseSqlite("Data Source=database.db;Foreign Keys=True");
        });
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        var app = builder.Build();
        
        app.UseCors();
        app.MapControllers();
        app.UseStaticFiles();
        app.MapFallbackToFile("index.html");
        app.MapHub<ChatHub>("/Chat");

        app.Run();

    }
}



