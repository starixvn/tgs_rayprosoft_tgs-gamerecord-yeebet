namespace Tgs.GameRecord.YeeBet.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;

    public string GameProviderConfig { get; set; } = default!;    

    public double LockMinute { get; set; } = 4d;
}
