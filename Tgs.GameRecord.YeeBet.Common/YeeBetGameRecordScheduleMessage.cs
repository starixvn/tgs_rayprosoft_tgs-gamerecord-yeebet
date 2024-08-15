using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.YeeBet.Common;

public record YeeBetGameRecordScheduleMessage
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
}
