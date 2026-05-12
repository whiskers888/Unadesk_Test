using System.Text;
using System.Text.Json;
using BackgroundWorker.UseCases.ProcessDocument;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using Shared.Messaging;

namespace BackgroundWorker.Infrastructure.Messaging.Consumer;

public class RabbitMqConsumer(
    IConfiguration config,
    RabbitMqConnectionManager connectionManager,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqConsumer> logger)
    : BackgroundService
{
    private readonly string _queueName = QueueNameHelper.GetQueueName(typeof(DocumentUploadedEvent),config["RabbitMQ:ServicePrefix"] ?? "pdf");
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel =  await connectionManager.CreateConsumerChannelAsync(_queueName);
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var deliveryTag = ea.DeliveryTag;
            try
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<DocumentUploadedEvent>(Encoding.UTF8.GetString(body));
                if (message != null)
                {
                    await ProcessDocumentAsync(message);
                    await _channel.BasicAckAsync(deliveryTag, false, stoppingToken);
                }
                else
                {
                    await _channel.BasicNackAsync(deliveryTag, false, false, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message");
                await _channel.BasicNackAsync(deliveryTag, false, false, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        logger.LogInformation("RabbitMQ consumer started on queue {QueueName}", _queueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessDocumentAsync(DocumentUploadedEvent evt)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        await mediator.Send(
            new ProcessDocumentCommand
                {
                    DocumentId = evt.DocumentId,
                    FileName =  evt.FileName,   
                    FilePath =  evt.FilePath,
                }
            );
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync(cancellationToken: cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}