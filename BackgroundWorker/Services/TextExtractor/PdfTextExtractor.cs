using System.Text;
using UglyToad.PdfPig;

namespace BackgroundWorker.Services.TextExtractor;

public class PdfTextExtractor : ITextExtractor
{
    public async Task<string> ExtractTextAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"PDF файл не найден по пути: {filePath}");

        return await Task.Run(() =>
        {
            using var pdf = PdfDocument.Open(filePath);
            var sb = new StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                sb.Append(page.Text);
            }
            return sb.ToString();
        });
    }
}