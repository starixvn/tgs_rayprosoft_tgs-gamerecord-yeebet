namespace Tgs.GameRecord.YeeBet.Core.DispatchServices;
public record ScheduleDispatchCommand(
    Uri ApiUrl,
    string AppId,
    string SecretKey);
