namespace Tgs.GameRecord.YeeBet.Messages;

public record ScheduleMessageModel(
    Uri ApiUrl,
    string AppId,
    string SecretKey);
