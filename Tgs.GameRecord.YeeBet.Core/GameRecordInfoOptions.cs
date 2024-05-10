namespace Tgs.GameRecord.YeeBet.Core;

public record GameRecordInfoOptions
{
	public string GameProviderCode { get; set; } = default!;

	public string GameProviderConfiguration { get; init; } = default!;

	public string HangfirePrefix { get; set; } = "gamerecord-yeebet-hangfire:";
	public string HangfireLocking { get; set; } = "gamerecord-yeebet-locking:";

	public string ScheduleLockingPrefix { get; set; } = "gamerecord-yeebet-schedule-locking:";

	public double ScheduleLockingMinute { get; set; } = 10d;
}
