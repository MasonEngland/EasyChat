using EasyChat.Models;
namespace EasyChat.Services;

public interface IFileService
{
    Task<bool> UploadFile(string roomId, IFormFile file, string user);
    Task<FileMessage[]> GetFilesForRoom(string roomId);
    Task<string> GetFilePath(string fileId);
}