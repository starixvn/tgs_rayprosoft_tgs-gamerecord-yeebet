namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;
public record ScheduleWorkerCommand(
    Uri ApiUrl,
    string AppId,
    string SecretKey);
