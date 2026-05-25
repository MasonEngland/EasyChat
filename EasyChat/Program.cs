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
            options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
        });
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:5173", "http://localhost:3000") //added localhost:3000 for testing with React dev server
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        var app = builder.Build();


        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            db.Database.Migrate();
        }
        
        app.UseCors();
        app.MapControllers();
        app.UseStaticFiles();
        app.MapFallbackToFile("index.html");
        app.MapHub<ChatHub>("/Chat");

        app.Run();

    }
}



