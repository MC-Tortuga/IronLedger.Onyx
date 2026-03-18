using System.Text.Json;
using IronLedger.Core.Events;
using IronLedger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace IronLedger.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost(
            "/payments/authorize",
            async ([FromBody] PaymentRequest req, OnyxDbContext db) =>
            {
                var paymentEvent = new PaymentAuthorized(Guid.NewGuid(), req.Amount, req.AccountId);

                var outboxMessage = new OutboxMessage
                {
                    Type = nameof(PaymentAuthorized),
                    Payload = JsonSerializer.Serialize(paymentEvent),
                };

                db.OutboxMessages.Add(outboxMessage);
                await db.SaveChangesAsync();

                return Results.Accepted($"/payments/{paymentEvent.PaymentId}", new { PaymentId = paymentEvent.PaymentId });
            }
        );
    }
}

public record PaymentRequest(decimal Amount, string AccountId);
