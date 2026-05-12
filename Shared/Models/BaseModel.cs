using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class BaseModel
{
    [Key]
    public Guid Id { get; init; }
    public DateTime UploadedAt { get; set; }
    
    private readonly List<object> _domainEvents = [];
    public IReadOnlyList<object> DomainEvents => _domainEvents;

    public void AddDomainEvent(object domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}