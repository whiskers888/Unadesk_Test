using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Unadesk_Test.UseCases.Document.Command;
using Unadesk_Test.UseCases.Document.Queries;

namespace Unadesk_Test.Controllers;

/// <summary>
/// Контроллер для работы с документами (загрузка, получение списка, извлечение текста)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DocumentsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Загружает PDF-файл для асинхронной обработки
    /// </summary>
    /// <param name="file">PDF-файл для загрузки</param>
    /// <returns>Возвращает идентификатор документа и статус 202 Accepted</returns>
    /// <response code="202">Документ принят в обработку</response>
    /// <response code="400">Файл не был загружен, либо это не PDF</response>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            var documentId = await mediator.Send(new UploadDocumentCommand { File = file });
            return Accepted(new { id = documentId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Получает список всех загруженных документов с их статусами
    /// </summary>
    /// <returns>Список документов (Id, FileName, Status, ProcessedAt)</returns>
    /// <response code="200">Список успешно получен</response>
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var documents = await mediator.Send(new GetDocumentListQuery());
        return Ok(documents);
    }

    /// <summary>
    /// Получает извлечённый текст из PDF по идентификатору документа
    /// </summary>
    /// <param name="id">Идентификатор документа (Guid)</param>
    /// <returns>Текст документа, если обработка завершена успешно</returns>
    /// <response code="200">Текст успешно получен</response>
    /// <response code="400">Документ ещё не обработан (статус Pending или Processing)</response>
    /// <response code="404">Документ с указанным ID не найден</response>
    [HttpGet("{id:guid}/text")]
    public async Task<IActionResult> GetText(Guid id)
    {
        var result = await mediator.Send(new GetDocumentTextQuery { DocumentId = id });
        if (result is null)
            return NotFound();
        if (result.Status is ProcessingStatus.Pending or ProcessingStatus.Processing)
            return BadRequest(new { Message = "Файл еще не готов", result.Status });
        return Ok(result);
    }
}