using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Shared.Messaging;

public class RabbitMqConnectionManager(IConfiguration config) : IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _publishChannel;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private async Task<IConnection> GetConnectionAsync()
    {
        if (_connection != null) return _connection;
        await _lock.WaitAsync();
        try
        {
            return _connection ??= await CreateConnectionAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<IChannel> GetPublishChannelAsync()
    {
        if (_publishChannel != null) return _publishChannel;
        await _lock.WaitAsync();
        try
        {
            return _publishChannel ??= await (await GetConnectionAsync()).CreateChannelAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeclareQueueAsync(string queueName)
    {
        var channel = await GetPublishChannelAsync();
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
    }

    public async Task PublishAsync(string queueName, byte[] body)
    {
        var channel = await GetPublishChannelAsync();
        await channel.BasicPublishAsync("", queueName, body);
    }

    public async Task<IChannel> CreateConsumerChannelAsync(string queueName)
    {
        var connection = await GetConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
        await channel.BasicQosAsync(0, 1, false);
        return channel;
    }

    private async Task<IConnection> CreateConnectionAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(config["RabbitMQ:Port"] ?? "5672")
        };
        return await factory.CreateConnectionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_publishChannel != null) await _publishChannel.CloseAsync();
        if (_connection != null) await _connection.CloseAsync();
    }
}