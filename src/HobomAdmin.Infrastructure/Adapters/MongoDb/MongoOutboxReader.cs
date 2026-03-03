using HobomAdmin.Application.Ports.Out;
using HobomAdmin.Domain.Entities;
using HobomAdmin.Domain.Enums;
using MongoDB.Driver;

namespace HobomAdmin.Infrastructure.Adapters.MongoDb;

public class MongoOutboxReader(IMongoDatabase database) : IOutboxReader
{
    private IMongoCollection<OutboxDocument> Collection
        => database.GetCollection<OutboxDocument>("outbox");

    public async Task<IReadOnlyList<OutboxEntry>> GetRecentAsync(int limit, CancellationToken ct = default)
    {
        var docs = await Collection
            .Find(FilterDefinition<OutboxDocument>.Empty)
            .SortByDescending(d => d.CreatedAt)
            .Limit(limit)
            .ToListAsync(ct);

        return docs.Select(d => d.ToEntity()).ToList();
    }

    public async Task<OutboxStatusSummary> GetStatusSummaryAsync(CancellationToken ct = default)
    {
        var pending = await Collection.CountDocumentsAsync(d => d.Status == "PENDING", cancellationToken: ct);
        var sent = await Collection.CountDocumentsAsync(d => d.Status == "SENT", cancellationToken: ct);
        var failed = await Collection.CountDocumentsAsync(d => d.Status == "FAILED", cancellationToken: ct);

        return new OutboxStatusSummary((int)pending, (int)sent, (int)failed);
    }
}

internal class OutboxDocument
{
    public string Id { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }

    public OutboxEntry ToEntity() => new()
    {
        Id = Id,
        MessageType = Enum.TryParse<MessageType>(MessageType, true, out var mt) ? mt : Domain.Enums.MessageType.MailMessage,
        Status = Enum.TryParse<OutboxStatus>(Status, true, out var st) ? st : OutboxStatus.Pending,
        Payload = Payload,
        CreatedAt = CreatedAt,
        SentAt = SentAt
    };
}
