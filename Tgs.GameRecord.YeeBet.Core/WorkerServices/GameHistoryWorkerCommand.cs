namespace Tgs.GameRecord.YeeBet.Core.WorkerServices;
public record GameHistoryWorkerCommand(
    Uri ApiUrl,
    string AppId,
    string SecretKey);
