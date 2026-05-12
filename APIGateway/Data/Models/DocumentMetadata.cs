using Shared.Enums;
using Shared.Messaging;
using Shared.Models;

namespace Unadesk_Test.Data.Models;

public class DocumentMetadata : BaseModel, IHasDomainEvents
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;
    public string? ExtractedText { get; set; }
    public string? ErrorMessage { get; set; }

}