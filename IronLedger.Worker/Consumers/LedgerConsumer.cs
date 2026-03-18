using IronLedger.Infrastructure.Messaging;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace IronLedger.Worker.Consumers;

public class LedgerConsumer(RabbitConnection rabbit) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var channel = await rabbit.GetChannelAsync();

        await channel.QueueDeclareAsync("ledger_queue", durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync("ledger_queue", "ledger_exchange", "ledger.#");
    }

}
