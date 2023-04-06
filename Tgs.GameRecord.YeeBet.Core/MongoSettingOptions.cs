namespace Tgs.GameRecord.YeeBet.Core;

public record MongoSettingOptions
{
    public string ManagerName { get; set; } = default!;
    public string QueueName { get; set; } = default!;
    public string GlobPattern { get; set; } = default!;
}