using Microsoft.AspNetCore.Http;

namespace Shared.Services.FileStorage;

public interface IFileStorage
{
    Task<string> SaveFileAsync(IFormFile file, Guid documentId, string? basePath = null);
    Task<bool> FileExistsAsync(string filePath);
    Task<byte[]> ReadFileAsync(string filePath);
    void DeleteFile(string filePath);
}