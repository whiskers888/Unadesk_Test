using MediatR;

namespace BackgroundWorker.UseCases.ProcessDocument;

public class ProcessDocumentCommand : IRequest
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}