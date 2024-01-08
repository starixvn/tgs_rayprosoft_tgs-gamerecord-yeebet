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

        logger.LogInformation("LogOn:{logOn} | Action:GameRecordInquireAsync | Request:{request}",
            DateTime.UtcNow.ToString("O"),
            JsonSerializer.Serialize(new
            {
                command.ApiUrl,
                command.AppId,
                command.SecretKey,
                command.StartTime,
                command.EndTime,
                pageIndex,
                pageSize
            }));

        var reply = await client.GameRecordInquireAsync(
            apiUrl: command.ApiUrl,
            appId: command.AppId,
            secretKey: command.SecretKey,
            beginTime: command.StartTime,
            endTime: command.EndTime,
            pageSize: pageSize,
            pageIndex: pageIndex,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation("LogOn:{logOn} | Action:GameRecordInquireAsync | Response:{response}",
            DateTime.UtcNow.ToString("O"),
            JsonSerializer.Serialize(reply));

        if (reply is null)
            return;

        if (reply.ArraySize is 0)
            return;

        foreach (var gameHistory in reply.Items ?? Enumerable.Empty<GameRecordInfo>())
        {
            try
            {
                await service
                    .ExecuteAsync(
                    command: new HandleRecordCommand(Record: gameHistory),
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), gameHistory);
            }
        }

        while ((pageIndex + 1) * pageSize < reply.Total)
        {
            pageIndex++;

            logger.LogInformation("LogOn:{logOn} | Action:GameRecordInquireAsync | Request:{request}",
                DateTime.UtcNow.ToString("O"),
                JsonSerializer.Serialize(new
                {
                    command.ApiUrl,
                    command.AppId,
                    command.SecretKey,
                    command.StartTime,
                    command.EndTime,
                    pageIndex,
                    pageSize,
                }));

            reply = await client
                .GameRecordInquireAsync(
                    command.ApiUrl,
                    command.AppId,
                    command.SecretKey,
                    command.StartTime,
                    command.EndTime,
                    pageSize: pageSize,
                    pageIndex: pageIndex,
                    cancellationToken)
                .ConfigureAwait(false);

            logger.LogInformation("LogOn:{logOn} | Action:GameRecordInquireAsync |  Response:{response}",
                DateTime.UtcNow.ToString("O"),
                JsonSerializer.Serialize(reply));

            foreach (var gameHistory in reply.Items ?? Enumerable.Empty<GameRecordInfo>())
            {
                try
                {
                    await service
                        .ExecuteAsync(
                        command: new HandleRecordCommand(Record: gameHistory),
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), gameHistory);
                }
            }
        }
    }
}
