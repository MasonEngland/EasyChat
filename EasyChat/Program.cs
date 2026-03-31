using EasyChat.Hubs;

namespace EasyChat;

public class EasyChat 
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSignalR();

        var app = builder.Build();


        app.MapGet("/hello", () => "Hello World!");
        app.UseStaticFiles();
        app.MapFallbackToFile("index.html");
        app.MapHub<ChatHub>("/Chat");

        app.Run();

    }
}



