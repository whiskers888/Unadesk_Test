using System;
using Shared.Enums;

namespace Shared.Events;

public class DocumentProcessedEvent
{
    public Guid DocumentId { get; set; }
    public ProcessingStatus Status { get; set; }
    public string? ExtractedText { get; set; }
    public string? ErrorMessage { get; set; }
}