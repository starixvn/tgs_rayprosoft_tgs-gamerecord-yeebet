using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameRecord.YeeBet.Common;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.YeeBet.Core.JobApplicationServices;

internal class JobExecuteCommandService(
	ILogger<JobExecuteCommandService> logger,
	IMessageSender messageSender) : ICommandService<JobExecuteCommand>
{
	public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
	{
		var parameters = new YeeBetGameRecordScheduleMessage
		{
			ApiUrl = command.ApiUrl,
			AppId = command.AppId,
			SecretKey = command.SecretKey,
		};

		logger.LogDebug("LogOn:{logOn} Parameters:{parameters}", DateTime.Now.ToString("u"), parameters);

		return messageSender
			.PublishAsync(
			subject: YeeBetNatsConst.SchedulePublish,
			data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(parameters)),
			cancellationToken: cancellationToken);
	}
}
