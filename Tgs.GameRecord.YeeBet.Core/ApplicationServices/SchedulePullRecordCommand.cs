namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;
public record SchedulePullRecordCommand(
    Uri ApiUrl,
    string AppId,
    string SecretKey);
