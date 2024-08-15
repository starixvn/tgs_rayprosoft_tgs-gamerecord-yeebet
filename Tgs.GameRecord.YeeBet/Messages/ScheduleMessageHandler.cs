using System.Text;
using System.Text.Json;
using Tgs.GameRecord.YeeBet.Common;
using Tgs.GameRecord.YeeBet.Core;
using Tgs.GameRecord.YeeBet.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.YeeBet.Messages;

internal class ScheduleMessageHandler(
	ILogger<ScheduleMessageHandler> logger,
	ISchedulePullRecordDispatcher dispatcher) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var json = Encoding.UTF8.GetString(data.Span);
		var message = JsonSerializer.Deserialize<YeeBetGameRecordScheduleMessage>(json)
			?? throw new NullReferenceException("The request data resolves to a null value");

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = subject,
				Data = new
				{
					message.ApiUrl,
					message.AppId,
					message.SecretKey
				},
				Message = "This receive message handler."
			}));

		return dispatcher.PushCommandAsync(
			command: new SchedulePullRecordCommand(
				ApiUrl: message.ApiUrl,
				AppId: message.AppId,
				SecretKey: message.SecretKey),
			cancellationToken: cancellationToken);
	}
}
