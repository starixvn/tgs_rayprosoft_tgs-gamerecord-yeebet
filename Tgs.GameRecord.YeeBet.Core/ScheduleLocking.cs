using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Tgs.GameRecord.YeeBet.Core;

internal class ScheduleLocking(
    IOptions<GameRecordInfoOptions> options,
    IDatabase database) : IScheduleLocking
{
    private readonly GameRecordInfoOptions _options = options.Value;

    public Task<bool> LockingAsync(string appId, CancellationToken cancellationToken = default)
            => database.StringSetAsync(
            key: GetKey(appId),
            value: new JsonObject
            {
                ["MachineName"] = Environment.MachineName,
                ["AppId"] = appId,
                ["LabelTime"] = DateTimeOffset.Now
            }.ToJsonString(),
            expiry: TimeSpan.FromMinutes(_options.ScheduleLockingMinute),
            when: When.NotExists);

    public Task<bool> UnlockedAsync(string appId, CancellationToken cancellationToken = default)
        => database.KeyDeleteAsync(
            key: GetKey(appId),
            flags: CommandFlags.FireAndForget);

    private string GetKey(string appId)
    => string.Concat(_options.ScheduleLockingPrefix, appId);
}
