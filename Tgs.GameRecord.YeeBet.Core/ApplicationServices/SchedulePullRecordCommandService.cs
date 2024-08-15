using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.YeeBet.Client;
using Tgs.GameInterface.YeeBet.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;

internal class SchedulePullRecordCommandService(
	ILogger<SchedulePullRecordCommandService> logger,
	IGameInterfaceYeeBetClient client,
	ICommandService<HandleRecordCommand> service) : ICommandService<SchedulePullRecordCommand>
{
	public async ValueTask ExecuteAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default)
	{
		IEnumerable<GameRecordInfo> gameRecords;
		do
		{
			gameRecords = await client.GameRecordAsync(
			   apiUrl: command.ApiUrl,
			   appId: command.AppId,
			   secretKey: command.SecretKey,
			   size: 2000,
			   cancellationToken: cancellationToken)
			   .ToArrayAsync(cancellationToken: cancellationToken)
			   .ConfigureAwait(false)
			   ?? Enumerable.Empty<GameRecordInfo>();

			var externalRoundIds = new List<string>();

			foreach (var gameRecord in gameRecords)
			{
				try
				{
					await service.ExecuteAsync(
						command: new HandleRecordCommand(Record: gameRecord),
						cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					externalRoundIds.Add(gameRecord.ExternalRoundId);
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

			if (externalRoundIds.Count != 0)
			{
				_ = await client.GameRecordMarkAsync(
					apiUrl: command.ApiUrl,
					appId: command.AppId,
					secretKey: command.SecretKey,
					externalRoundIds: externalRoundIds,
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}
		} while (gameRecords.Any());
	}
}
