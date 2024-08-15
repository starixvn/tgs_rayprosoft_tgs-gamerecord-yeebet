using Tgs.GameRecord.YeeBet.Core.JobApplicationServices;

namespace Tgs.GameRecord.YeeBet.Core;

public interface IHangfireUtensilRepository
{
	public Task JobCheckAsync();

	public Task JobExecuteAsync(JobExecuteCommand source);

	public Task JobInitAsync();
}
