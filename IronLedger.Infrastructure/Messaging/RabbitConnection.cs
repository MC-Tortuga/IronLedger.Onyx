using System.Text;
using RabbitMQ.Client;

namespace IronLedger.Infrastructure.Messaging;

public class RabbitConnection : IAsyncDisposable
{
    private readonly ConnectionFactory _factory;
    private IConnection _connection;
    private IChannel _channel;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitConnection(string hostName)
    {
        _factory = new ConnectionFactory { HostName = hostName };
    }

    public async Task<IChannel> GetChannelAsync()
    {
        if (_channel is not null)
            return _channel;
        await _lock.WaitAsync();

        try
        {
            if (_channel is null)
            {
                _connection = await _factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(
                    exchange: "onyx_events",
                    type: ExchangeType.Direct,
                    durable: true
                );
            }
            return _channel;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task PublishAsync(string routingKey, string message)
    {
        var channel = await GetChannelAsync();
        var body = Encoding.UTF8.GetBytes(message);

        var properties = new BasicProperties { Persistent = true };

        await channel.BasicPublishAsync(
            exchange: "onyx_events",
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.CloseAsync();
        if (_connection is not null)
            await _connection.CloseAsync();
        _lock.Dispose();
    }
}
