using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using Shared.Messaging;
using Unadesk_Test.Data;

namespace Unadesk_Test.Infrastructure.Messaging.Consumer;

public class RabbitMqEventConsumer(
    IConfiguration config,
    RabbitMqConnectionManager connectionManager,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqEventConsumer> logger)
    : BackgroundService
{
    private readonly string _queueName = QueueNameHelper.GetQueueName(typeof(DocumentProcessedEvent),config["RabbitMQ:ServicePrefix"] ?? "pdf");
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await connectionManager.CreateConsumerChannelAsync(_queueName);
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<DocumentProcessedEvent>(Encoding.UTF8.GetString(body));
                if (message != null)
                {
                    await ProcessEventAsync(message);
                    await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                else
                {
                    await _channel!.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing event");
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        logger.LogInformation("Event consumer started on queue {QueueName}", _queueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessEventAsync(DocumentProcessedEvent evt)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        var doc = await db.Documents.FindAsync(evt.DocumentId);
        if (doc != null)
        {
            doc.Status = evt.Status;
            doc.ExtractedText = evt.ExtractedText;
            doc.ErrorMessage = evt.ErrorMessage;
            await db.SaveChangesAsync();
            logger.LogInformation("Document {DocumentId} updated with status {Status}", evt.DocumentId, evt.Status);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}