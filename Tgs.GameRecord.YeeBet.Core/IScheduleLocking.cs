namespace Tgs.GameRecord.YeeBet.Core;

public interface IScheduleLocking
{
	public Task<bool> LockingAsync(string appId, CancellationToken cancellationToken = default);

	public Task<bool> UnlockedAsync(string appId, CancellationToken cancellationToken = default);
}
