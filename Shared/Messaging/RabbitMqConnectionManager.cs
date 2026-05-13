using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Shared.Messaging;

public class RabbitMqConnectionManager(IConfiguration config) : IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _publishChannel;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly SemaphoreSlim _channelLock = new(1, 1);

    private async Task<IConnection> GetConnectionAsync()
    {
        if (_connection != null) return _connection;
        await _connectionLock.WaitAsync();
        try
        {
            return _connection ??= await CreateConnectionAsync();
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task<IChannel> GetPublishChannelAsync()
    {
        if (_publishChannel != null) return _publishChannel;
        await _channelLock.WaitAsync();
        try
        {
            if (_publishChannel != null) return _publishChannel; // double-check
            var connection = await GetConnectionAsync(); // здесь уже не захватывает _channelLock
            _publishChannel = await connection.CreateChannelAsync();
            return _publishChannel;
        }
        finally
        {
            _channelLock.Release();
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
        if (_publishChannel != null)
        {
            await _publishChannel.CloseAsync();
            _publishChannel.Dispose();
        }
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
        _connectionLock.Dispose();
        _channelLock.Dispose();
    }
}