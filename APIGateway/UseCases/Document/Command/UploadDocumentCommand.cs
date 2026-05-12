using MediatR;

namespace Unadesk_Test.UseCases.Document.Command;

public class UploadDocumentCommand : IRequest<Guid>
{
    public IFormFile File { get; init; } = null!;
}