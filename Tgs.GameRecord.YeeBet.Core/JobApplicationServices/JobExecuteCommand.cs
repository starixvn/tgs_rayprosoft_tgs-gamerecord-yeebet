namespace Tgs.GameRecord.YeeBet.Core.JobApplicationServices;

public record JobExecuteCommand(
    string GameProviderCode,
    string Name,
    Uri ApiUrl,
    string AppId,
    string SecretKey);
