using EasyChat.Context;
using EasyChat.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyChat.Services;


public class FileService : IFileService
{
    private readonly DatabaseContext _db;

    public FileService(DatabaseContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<bool> UploadFile(string roomId, IFormFile file, string user)
    {
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", roomId);
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }
        string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        };

        FileMessage fileMessage = new FileMessage
        {
            FileName = file.FileName,
            FilePath = $"/uploads/{roomId}/{uniqueFileName}",
            RoomId = roomId,
            User = user
        };

        _db.Files.Add(fileMessage);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<FileMessage[]> GetFilesForRoom(string roomId)
    {
        return await _db.Files.Where(f => f.RoomId == roomId).ToArrayAsync();
    }

    public async Task<string?> GetFilePath(string fileId)
    {
        FileMessage? fileMessage = await _db.Files.FindAsync(fileId);
        if (fileMessage == null)
        {
           return null;
        }

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileMessage.FilePath.TrimStart('/'));
        return filePath;
    }
}