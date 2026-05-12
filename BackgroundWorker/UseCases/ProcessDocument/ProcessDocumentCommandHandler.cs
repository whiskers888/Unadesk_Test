using BackgroundWorker.Data;
using BackgroundWorker.Data.Models;
using BackgroundWorker.Services.TextExtractor;
using MediatR;
using Shared.Enums;
using Shared.Events;

namespace BackgroundWorker.UseCases.ProcessDocument;


public class ProcessDocumentCommandHandler(
    BackgroundWorkerDbContext db,
    ITextExtractor textExtractor,
    ILogger<ProcessDocumentCommandHandler> logger)
    : IRequestHandler<ProcessDocumentCommand>
{
    public async Task Handle(ProcessDocumentCommand request, CancellationToken cancellationToken)
    {
        var existing = await db.Documents.FindAsync([request.DocumentId], cancellationToken);
        if (existing != null)
        {
            logger.LogInformation("Document {FileName} already processed", request.FileName);
            return;
        }

        var document = new Document
        {
            Id = request.DocumentId,
            FileName = request.FileName,
            Status = ProcessingStatus.Processing
        };
        db.Documents.Add(document);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            document.ExtractedText = await textExtractor.ExtractTextAsync(request.FilePath);
            document.Status = ProcessingStatus.Completed;
        }
        catch (Exception ex)
        {
            document.Status = ProcessingStatus.Failed;
            document.ErrorMessage = ex.Message;
            logger.LogError(ex, "Extraction failed for {FilePath}", request.FilePath);
        }
        var metadata = new DocumentProcessedEvent
        {
            DocumentId = request.DocumentId,
            Status = document.Status,
            ExtractedText = document.ExtractedText,
            ErrorMessage = document.ErrorMessage
        };
        document.AddDomainEvent(metadata);
        logger.LogInformation("Published DocumentProcessedEvent for {DocumentId}", request.DocumentId);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Document {DocumentId} finished with status {Status}", request.DocumentId, document.Status);
    }
}