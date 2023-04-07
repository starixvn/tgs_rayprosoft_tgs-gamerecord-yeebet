namespace Tgs.GameRecord.YeeBet.Messages;

public record ManualMessageModel(
    Uri ApiUrl,
    string AppId,
    string SecretKey,
    DateTimeOffset BeginTime,
    DateTimeOffset EndTime);
