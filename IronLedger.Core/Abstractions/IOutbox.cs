namespace IronLedger.Core.Abstractions;

public interface IOutbox
{
    Task EnqueueAsync<T>(T message, Guid correlationId)
        where T : class;
}
