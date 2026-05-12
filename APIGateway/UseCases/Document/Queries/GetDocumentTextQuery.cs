using MediatR;
using Unadesk_Test.UseCases.Document.DTO;

namespace Unadesk_Test.UseCases.Document.Queries;

public class GetDocumentTextQuery : IRequest<DocumentTextDto?>
{
    public Guid DocumentId { get; init; }
}