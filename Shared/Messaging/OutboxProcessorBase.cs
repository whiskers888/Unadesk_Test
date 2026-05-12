using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Shared.Messaging;

public abstract class OutboxProcessorBase<TDbContext>(
    IServiceScopeFactory scopeFactory,
    ILogger logger,
    RabbitMqConnectionManager connectionManager,
    string servicePrefix)
    : BackgroundService
    where TDbContext : DbContext
{
    protected abstract DbSet<OutboxMessage> GetOutboxSet(TDbContext dbContext);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            await ProcessOutboxMessagesAsync(stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var outboxSet = GetOutboxSet(db);

        var messages = await outboxSet
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    logger.LogWarning("Unknown event type: {Type}", message.Type);
                    message.ProcessedAt = DateTime.UtcNow;
                    continue;
                }
                var queueName = QueueNameHelper.GetQueueName(eventType, servicePrefix);
                await connectionManager.DeclareQueueAsync(queueName);
                var body = Encoding.UTF8.GetBytes(message.Content);
                await connectionManager.PublishAsync(queueName, body);

                message.ProcessedAt = DateTime.UtcNow;
                logger.LogInformation("Outbox message {Id} sent to queue {Queue}", message.Id, queueName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send outbox message {Id}", message.Id);
                message.RetryCount++;
                if (message.RetryCount >= 5)
                {
                    logger.LogWarning("Outbox message {Id} exceeded retry limit", message.Id);
                    message.ProcessedAt = DateTime.UtcNow;
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}