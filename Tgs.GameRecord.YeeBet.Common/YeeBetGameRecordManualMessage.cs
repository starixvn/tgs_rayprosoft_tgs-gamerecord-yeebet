using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.YeeBet.Common;
public record YeeBetGameRecordManualMessage
{
	[Required]
	[DataType(DataType.Url)]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	public string AppId { get; init; } = default!;
	[Required]
	public string SecretKey { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	public DateTimeOffset StartTime { get; init; }
	[Required]
	[DataType(DataType.DateTime)]
	public DateTimeOffset EndTime { get; init; }
}
