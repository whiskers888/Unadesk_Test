using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Shared.Services.FileStorage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;

    public LocalFileStorage(IConfiguration config, IHostEnvironment env)
    {
        var folder = config["FileStorage:BasePath"] ?? "Uploads";
        _basePath = Path.Combine(env.ContentRootPath, folder);
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(IFormFile file, Guid documentId, string? basePath = null)
    {
        var path = basePath ?? _basePath;
        var fileName = $"{documentId}.pdf";
        var filePath = Path.Combine(path, fileName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return filePath;
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        return Task.FromResult(File.Exists(filePath));
    }

    public Task<byte[]> ReadFileAsync(string filePath)
    {
        return Task.FromResult(File.ReadAllBytes(filePath));
    }

    public void DeleteFile(string filePath)
    {
        if (File.Exists(filePath)) File.Delete(filePath);
    }
}