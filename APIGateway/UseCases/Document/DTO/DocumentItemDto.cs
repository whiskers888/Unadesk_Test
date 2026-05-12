using Shared.Enums;

namespace Unadesk_Test.UseCases.Document.DTO;

public class DocumentItemDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public ProcessingStatus Status { get; set; }
}