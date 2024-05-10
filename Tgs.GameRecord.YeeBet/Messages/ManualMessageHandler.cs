using System.Text;
using System.Text.Json;
using Tgs.GameRecord.YeeBet.Common;
using Tgs.GameRecord.YeeBet.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.YeeBet.Messages;

internal class ManualMessageHandler(
	ILogger<ManualMessageHandler> logger,
	ICommandService<ManualPullRecordCommand> service) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var json = Encoding.UTF8.GetString(data.Span);
		var message = JsonSerializer.Deserialize<YeeBetGameRecordManualMessage>(json)
			?? throw new NullReferenceException("The request data resolves to a null value");

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{message}",
			DateTime.Now.ToString("o"),
			subject,
			message);

		var command = new ManualPullRecordCommand(
			ApiUrl: message.ApiUrl,
			AppId: message.AppId,
			SecretKey: message.SecretKey,
			StartTime: message.StartTime,
			EndTime: message.EndTime);

		return service.ExecuteAsync(
			command: command,
			cancellationToken: cancellationToken);
	}
}
