using MediatR;
using Shared.Enums;
using Shared.Events;
using Shared.Services.FileStorage;
using Unadesk_Test.Data;
using Unadesk_Test.Data.Models;
using Unadesk_Test.UseCases.Document.Command;

namespace Unadesk_Test.UseCases.Document.Handler;


public class UploadDocumentCommandHandler(
    ApiDbContext db,
    IFileStorage fileStorage,
    ILogger<UploadDocumentCommandHandler> logger)
    : IRequestHandler<UploadDocumentCommand, Guid>
{

    public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var file = request.File;
        if (file == null || file.Length == 0)
            throw new ArgumentException("Файл не загружен");
        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Для загрузки доступны только PDF  файлы");

        var id = Guid.NewGuid();
        var filePath = await fileStorage.SaveFileAsync(file, id);

        var metadata = new DocumentMetadata
        {
            Id = id,
            FileName = file.FileName,
            FilePath = filePath,
            UploadedAt = DateTime.UtcNow,
            Status = ProcessingStatus.Pending
        }; 
        metadata.AddDomainEvent(new DocumentUploadedEvent {DocumentId = metadata.Id, FileName = metadata.FileName, FilePath = metadata.FilePath});

        await db.Documents.AddAsync(metadata, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Document {DocumentId} uploaded and event published", id);
        return id;
    }
}