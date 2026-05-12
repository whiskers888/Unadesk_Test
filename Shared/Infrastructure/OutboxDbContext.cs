using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shared.Messaging;
using Shared.Models;

namespace Shared.Infrastructure;

public abstract class OutboxDbContext(DbContextOptions options)
    : DbContext(options)
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var outboxMessages = new List<OutboxMessage>();
        
        foreach (var entity in entitiesWithEvents)
            foreach (var domainEvent in entity.DomainEvents)
                outboxMessages.Add(new OutboxMessage
                {
                    Type = domainEvent.GetType().AssemblyQualifiedName!,
                    Content = JsonSerializer.Serialize(domainEvent),
                    CreatedAt = DateTime.UtcNow,
                    RetryCount = 0
                });

        if (outboxMessages.Any())
            await OutboxMessages.AddRangeAsync(outboxMessages, cancellationToken);

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var entity in entitiesWithEvents)
            entity.ClearDomainEvents();

        return result;
    }
}