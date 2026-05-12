using MediatR;
using Microsoft.EntityFrameworkCore;
using Unadesk_Test.Data;
using Unadesk_Test.UseCases.Document.DTO;
using Unadesk_Test.UseCases.Document.Queries;

namespace Unadesk_Test.UseCases.Document.Handler;

public class GetDocumentTextQueryHandler(ApiDbContext db) : IRequestHandler<GetDocumentTextQuery, DocumentTextDto?>
{
    public async Task<DocumentTextDto?> Handle(GetDocumentTextQuery request, CancellationToken cancellationToken)
    {
        var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken: cancellationToken);
        if (doc is null) throw new ArgumentException("Файл не найден");

        return new DocumentTextDto
        {
            Id = doc.Id,
            FileName = doc.FileName,
            ExtractedText = doc.ExtractedText,
            Status = doc.Status,
        };
    }
}