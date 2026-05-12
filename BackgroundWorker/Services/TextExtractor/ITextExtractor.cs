namespace BackgroundWorker.Services.TextExtractor;

public interface ITextExtractor
{
    Task<string> ExtractTextAsync(string filePath);
}