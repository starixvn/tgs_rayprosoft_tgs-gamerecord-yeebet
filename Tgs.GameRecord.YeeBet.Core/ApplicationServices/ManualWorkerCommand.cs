namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;
public record ManualWorkerCommand(
    Uri ApiUrl,
    string AppId,
    string SecretKey,
    DateTimeOffset BeginTime,
    DateTimeOffset EndTime);
