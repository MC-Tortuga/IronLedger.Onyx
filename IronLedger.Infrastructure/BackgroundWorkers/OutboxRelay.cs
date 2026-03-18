using IronLedger.Infrastructure.Messaging;
using IronLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IronLedger.Infrastructure.BackgroundWorkers;

public class OutboxRelay(IServiceProvider serviceProvider, RabbitConnection rabbit)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OnyxDbContext>();

            var messages = await dbContext
                .OutboxMessages.Where(m => m.ProcessedAtUtc == null)
                .Take(50)
                .ToListAsync(ct);

            foreach (var message in messages)
            {
                await rabbit.PublishAsync(message.Type, message.Payload);

                message.ProcessedAtUtc = DateTime.UtcNow;
            }

            if (messages.Any())
            {
                await dbContext.SaveChangesAsync(ct);
            }

            await Task.Delay(1000, ct);
        }
    }
}
