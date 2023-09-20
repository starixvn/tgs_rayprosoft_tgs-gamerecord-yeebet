namespace Tgs.GameRecord.YeeBet.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;

    public string GameProviderConfiguration { get; init; } = default!;

    public string GameProviderHangfire { get; init; } = default!;

    public double LockMinute { get; set; } = 4d;
}
