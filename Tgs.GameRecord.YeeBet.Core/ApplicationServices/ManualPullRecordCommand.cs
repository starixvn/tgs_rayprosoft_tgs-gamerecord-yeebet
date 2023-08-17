namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;
public record ManualPullRecordCommand(
    Uri ApiUrl,
    string AppId,
    string SecretKey,
    DateTimeOffset BeginTime,
    DateTimeOffset EndTime);
