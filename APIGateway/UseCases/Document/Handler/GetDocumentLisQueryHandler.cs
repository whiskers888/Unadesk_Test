using MediatR;
using Microsoft.EntityFrameworkCore;
using Unadesk_Test.Data;
using Unadesk_Test.UseCases.Document.DTO;
using Unadesk_Test.UseCases.Document.Queries;

namespace Unadesk_Test.UseCases.Document.Handler;

public class GetDocumentListQueryHandler(ApiDbContext db) : IRequestHandler<GetDocumentListQuery, List<DocumentItemDto>>
{
    public async Task<List<DocumentItemDto>> Handle(GetDocumentListQuery request, CancellationToken ct)
    {
        return await db.Documents
            .Select(d => new DocumentItemDto { Id = d.Id, FileName = d.FileName, Status = d.Status })
            .ToListAsync(ct);
    }
}