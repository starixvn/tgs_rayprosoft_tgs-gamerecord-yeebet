using Tgs.GameRecord.YeeBet.Core.JobApplicationServices;

namespace Tgs.GameRecord.YeeBet.Core;

public interface IHangfireUtensil
{
	public Task JobCheckAsync();

	public Task JobExecuteAsync(JobExecuteCommand source);

	public Task JobInitAsync();
}
