namespace Shared.Events;

public class DocumentUploadedEvent
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}