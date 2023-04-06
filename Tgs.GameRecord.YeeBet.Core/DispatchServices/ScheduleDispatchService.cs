using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.DispatchServices;

internal class ScheduleDispatchService : ICommandService<ScheduleDispatchCommand>
{
    public ScheduleDispatchService()
    {
    }

    public ValueTask ExecuteAsync(ScheduleDispatchCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
