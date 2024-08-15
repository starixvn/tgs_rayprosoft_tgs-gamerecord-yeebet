using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.YeeBet.Common;
public record YeeBetGameRecordManualMessage
{
	[Required]
	[DataType(DataType.Url)]
	[DefaultValue(typeof(Uri), "http://api.98kbus.top/")]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "xtdA15OTVGEP")]
	public string AppId { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "4885366DF30D880224FF6AA46FBDE4F0")]
	public string SecretKey { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2023-04-10T10:30:08+08:00")]
	public DateTimeOffset StartTime { get; init; }
	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2023-04-10T10:40:08+08:00")]
	public DateTimeOffset EndTime { get; init; }
}
