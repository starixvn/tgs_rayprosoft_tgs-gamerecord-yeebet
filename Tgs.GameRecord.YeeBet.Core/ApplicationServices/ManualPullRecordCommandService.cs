using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.YeeBet.Client;
using Tgs.GameInterface.YeeBet.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;

internal class ManualPullRecordCommandService : ICommandService<ManualPullRecordCommand>
{
    private readonly IGameInterfaceYeeBetClient _client;
    private readonly ILogger<ManualPullRecordCommandService> _logger;
    private readonly ICommandService<HandleRecordCommand> _service;

    public ManualPullRecordCommandService(
        ILogger<ManualPullRecordCommandService> logger,
        IGameInterfaceYeeBetClient client,
        ICommandService<HandleRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(ManualPullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var pageIndex = 0;
        var pageSize = 2000;

        _logger.LogInformation("LogOn:{logOn} | Action:GameRecordInquireAsync | Request:{request}",
            DateTime.UtcNow.ToString("O"),
            JsonSerializer.Serialize(new
            {
                command.ApiUrl,
                command.AppId,
                command.SecretKey,
                command.BeginTime,
                command.EndTime,
                pageIndex,
                pageSize
            }));

        var reply = await _client
            .GameRecordInquireAsync(
                command.ApiUrl,
                command.AppId,
                command.SecretKey,
                command.BeginTime,
                command.EndTime,
                pageSize: pageSize,
                pageIndex: pageIndex,
                cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("LogOn:{logOn} | Action:GameRecordInquireAsync | Response:{response}",
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
                await _service
                    .ExecuteAsync(
                    command: new HandleRecordCommand(Record: gameHistory),
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), gameHistory);
            }
        }

        while ((pageIndex + 1) * pageSize < reply.Total)
        {
            pageIndex++;

            _logger.LogInformation("LogOn:{logOn} | Action:GameRecordInquireAsync | Request:{request}",
                DateTime.UtcNow.ToString("O"),
                JsonSerializer.Serialize(new
                {
                    command.ApiUrl,
                    command.AppId,
                    command.SecretKey,
                    command.BeginTime,
                    command.EndTime,
                    pageIndex,
                    pageSize,
                }));

            reply = await _client
                .GameRecordInquireAsync(
                    command.ApiUrl,
                    command.AppId,
                    command.SecretKey,
                    command.BeginTime,
                    command.EndTime,
                    pageSize: pageSize,
                    pageIndex: pageIndex,
                    cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("LogOn:{logOn} | Action:GameRecordInquireAsync |  Response:{response}",
                DateTime.UtcNow.ToString("O"),
                JsonSerializer.Serialize(reply));

            foreach (var gameHistory in reply.Items ?? Enumerable.Empty<GameRecordInfo>())
            {
                try
                {
                    await _service
                        .ExecuteAsync(
                        command: new HandleRecordCommand(Record: gameHistory),
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), gameHistory);
                }
            }
        }
    }
}
