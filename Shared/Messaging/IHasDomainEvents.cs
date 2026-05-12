namespace Shared.Messaging;

public interface IHasDomainEvents
{
    IReadOnlyList<object> DomainEvents { get; }
    void ClearDomainEvents();
}