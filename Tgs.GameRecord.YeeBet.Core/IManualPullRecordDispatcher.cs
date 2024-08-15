using Tgs.GameRecord.YeeBet.Core.ApplicationServices;

namespace Tgs.GameRecord.YeeBet.Core;

public interface IManualPullRecordDispatcher
{
	ValueTask PushCommandAsync(ManualPullRecordCommand command, CancellationToken cancellationToken = default);
}
