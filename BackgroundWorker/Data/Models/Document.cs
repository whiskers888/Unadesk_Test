using Shared.Enums;
using Shared.Messaging;
using Shared.Models;

namespace BackgroundWorker.Data.Models;

public class Document:BaseModel,IHasDomainEvents
{
    public string FileName { get; set; } = string.Empty;
    public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;
    public string? ExtractedText { get; set; }
    public string? ErrorMessage { get; set; }
}