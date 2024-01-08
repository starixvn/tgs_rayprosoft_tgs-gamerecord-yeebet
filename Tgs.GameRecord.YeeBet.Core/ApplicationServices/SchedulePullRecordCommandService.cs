using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.YeeBet.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;

internal class SchedulePullRecordCommandService(
    ILogger<SchedulePullRecordCommandService> logger,
    IScheduleLocking scheduleLocking,
    IGameInterfaceYeeBetClient client,
    ICommandService<HandleRecordCommand> service) : ICommandService<SchedulePullRecordCommand>
{
    public async ValueTask ExecuteAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        if (!await scheduleLocking.LockingAsync(
            command.AppId,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false))
        {
            logger.LogWarning("LogOn:{logOn} Message: This AppId({appId}) has been locked.", DateTime.Now.ToString("o"), command.AppId);
        }

        try
        {
            var gameHistorys = await client
                .GameRecordAsync(
                apiUrl: command.ApiUrl,
                appId: command.AppId,
                secretKey: command.SecretKey,
                size: 2000,
                cancellationToken: cancellationToken)
                .ToArrayAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            logger.LogInformation("LogOn:{logOn} | Action:GameRecordAsync | Response:{response}",
                DateTime.UtcNow.ToString("O"),
                JsonSerializer.Serialize(gameHistorys));

            if (gameHistorys.Length == 0)
                return;

            var externalRoundIds = new List<string>();

            foreach (var gameHistory in gameHistorys)
            {
                try
                {
                    await service
                        .ExecuteAsync(
                        command: new HandleRecordCommand(Record: gameHistory),
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    externalRoundIds.Add(gameHistory.ExternalRoundId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), gameHistory);
                }
            }

            if (externalRoundIds.Count != 0)
            {
                _ = await client
                    .GameRecordMarkAsync(
                    apiUrl: command.ApiUrl,
                    appId: command.AppId,
                    secretKey: command.SecretKey,
                    externalRoundIds: externalRoundIds,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LogOn:{logOn} Message: An exception occurs.({command})", DateTime.Now.ToString("o"), command);
        }
        finally
        {
            _ = await scheduleLocking.UnlockedAsync(
                appId: command.AppId,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
