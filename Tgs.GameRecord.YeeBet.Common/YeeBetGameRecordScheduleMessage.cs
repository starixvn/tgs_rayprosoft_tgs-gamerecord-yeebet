using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.YeeBet.Common;

public record YeeBetGameRecordScheduleMessage
{
	[Required]
	[DataType(DataType.Url)]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	public string AppId { get; init; } = default!;
	[Required]
	public string SecretKey { get; init; } = default!;
}
