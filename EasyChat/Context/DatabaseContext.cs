using Microsoft.EntityFrameworkCore;
using EasyChat.Models;

namespace EasyChat.Context;


public class DatabaseContext : DbContext
{
    public DbSet<Message> Messages { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<FileMessage> Files { get; set; }
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>()
            .HasIndex(m => new {m.RoomId, m.Timestamp});

        modelBuilder.Entity<Room>()
            .HasMany(r => r.Messages)
            .WithOne(m => m.Room)
            .HasForeignKey(m => m.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FileMessage>()
            .HasIndex(f => new {f.RoomId, f.Timestamp});

    }

}