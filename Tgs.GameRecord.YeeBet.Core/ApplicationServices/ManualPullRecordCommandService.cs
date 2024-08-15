using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.YeeBet.Client;
using Tgs.GameInterface.YeeBet.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;

internal class ManualPullRecordCommandService(
	ILogger<ManualPullRecordCommandService> logger,
	IGameInterfaceYeeBetClient client,
	ICommandService<HandleRecordCommand> service) : ICommandService<ManualPullRecordCommand>
{
	public async ValueTask ExecuteAsync(ManualPullRecordCommand command, CancellationToken cancellationToken = default)
	{
		var pageIndex = 0;
		var pageSize = 2000;
		GameRecordInfoReply reply;

		do
		{
			reply = await client.GameRecordInquireAsync(
				apiUrl: command.ApiUrl,
				appId: command.AppId,
				secretKey: command.SecretKey,
				beginTime: command.StartTime,
				endTime: command.EndTime,
				pageSize: pageSize,
				pageIndex: pageIndex,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (reply is not null)
			{
				foreach (var gameRecord in reply.Items ?? Enumerable.Empty<GameRecordInfo>())
				{
					try
					{
						await service.ExecuteAsync(
							command: new HandleRecordCommand(Record: gameRecord),
							cancellationToken: cancellationToken)
							.ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "{logError}",
							JsonSerializer.Serialize(new
							{
								LogOn = DateTime.Now.ToString("u"),
								Data = new
								{
									gameRecord.PlayerCode,
									gameRecord.ExternalRoundId,
									gameRecord.ExternalGameId,
									ExternalCreatedOn = gameRecord.ExternalCreatedOn.ToDateTimeOffset(),
									ExternalClosedOn = gameRecord.ExternalClosedOn.ToDateTimeOffset()
								},
								Message = "An exception occurred in this game wager."
							}));
					}
				}
			}
			pageIndex++;
		} while (reply is not null && (reply.Items ?? Enumerable.Empty<GameRecordInfo>()).Any());
	}
}
