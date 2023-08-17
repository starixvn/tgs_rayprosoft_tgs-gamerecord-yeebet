namespace Tgs.GameRecord.YeeBet.ViewModels;

public record ManualQueryViewModel(
    Uri ApiUrl,
    string AppId,
    string SecretKey,
    DateTimeOffset BeginTime,
    DateTimeOffset EndTime);
