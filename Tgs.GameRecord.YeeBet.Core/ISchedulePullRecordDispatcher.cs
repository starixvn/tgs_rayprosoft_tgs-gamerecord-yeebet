using Tgs.GameRecord.YeeBet.Core.ApplicationServices;

namespace Tgs.GameRecord.YeeBet.Core;

public interface ISchedulePullRecordDispatcher
{
	ValueTask PushCommandAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default);
}
