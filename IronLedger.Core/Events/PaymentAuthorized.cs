namespace IronLedger.Core.Events;

public record PaymentAuthorized(Guid PaymentId, decimal Amount, string AccountId);
