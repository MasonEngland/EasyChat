using Microsoft.EntityFrameworkCore;
using EasyChat.Context;


public class RoomCleanupWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RoomCleanupWorker> _logger;

    public RoomCleanupWorker(IServiceScopeFactory scopeFactory, ILogger<RoomCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Room cleanup worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupRoomsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during room cleanup");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); 
            // 5 min is plenty since expiration is 3 days
        }
    }

    private async Task CleanupRoomsAsync(CancellationToken token)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var cutoff = DateTime.UtcNow.AddDays(-3);

        var expiredRooms = await db.Rooms
            .Where(r => r.lastActive < cutoff)
            .ToListAsync(token);

        if (!expiredRooms.Any())
            return;

        _logger.LogInformation("Deleting {count} expired rooms", expiredRooms.Count);

        // Delete files BEFORE removing DB rows
        foreach (var room in expiredRooms)
        {
            DeleteRoomFiles(room.Id);
        }

        db.Rooms.RemoveRange(expiredRooms);

        await db.SaveChangesAsync(token);
    }

    private void DeleteRoomFiles(string roomId)
    {
        var path = Path.Combine("uploads", roomId);

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}