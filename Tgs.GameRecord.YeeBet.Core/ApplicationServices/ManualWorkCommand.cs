namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;
public record ManualWorkCommand(
    Uri ApiUrl,
    string AppId,
    string SecretKey,
    DateTimeOffset BeginTime,
    DateTimeOffset EndTime);
