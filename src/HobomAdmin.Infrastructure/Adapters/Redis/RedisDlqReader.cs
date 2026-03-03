using HobomAdmin.Application.Ports.Out;
using HobomAdmin.Domain.Entities;
using StackExchange.Redis;

namespace HobomAdmin.Infrastructure.Adapters.Redis;

public class RedisDlqReader(IConnectionMultiplexer redis) : IDlqReader
{
    public async Task<IReadOnlyList<string>> GetKeysAsync(string prefix, CancellationToken ct = default)
    {
        var server = redis.GetServers().First();
        var keys = server.Keys(pattern: $"{prefix}*").Select(k => k.ToString()).ToList();
        return await Task.FromResult<IReadOnlyList<string>>(keys);
    }

    public async Task<DlqEntry?> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return null;

        var parts = key.Split(':');
        return new DlqEntry
        {
            Key = key,
            Payload = value.ToString(),
            Category = parts.Length > 1 ? parts[1] : string.Empty,
            EventId = parts.Length > 2 ? parts[2] : string.Empty
        };
    }
}
