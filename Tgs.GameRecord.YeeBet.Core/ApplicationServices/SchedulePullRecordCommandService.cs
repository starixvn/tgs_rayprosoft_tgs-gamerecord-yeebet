using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.YeeBet.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;

internal class SchedulePullRecordCommandService : ICommandService<SchedulePullRecordCommand>
{
    private readonly IGameInterfaceYeeBetClient _client;
    private readonly ILogger<SchedulePullRecordCommandService> _logger;
    private readonly ICommandService<HandleRecordCommand> _service;

    public SchedulePullRecordCommandService(
        ILogger<SchedulePullRecordCommandService> logger,
        IGameInterfaceYeeBetClient client,
        ICommandService<HandleRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var gameHistorys = await _client
            .GameRecordAsync(
            apiUrl: command.ApiUrl,
            appId: command.AppId,
            secretKey: command.SecretKey,
            size: 2000,
            cancellationToken: cancellationToken)
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("LogOn:{logOn} | Action:GameRecordAsync | Response:{response}",
            DateTime.UtcNow.ToString("O"),
            JsonSerializer.Serialize(gameHistorys));

        if (!gameHistorys.Any())
            return;

        var externalRoundIds = new List<string>();

        foreach (var gameHistory in gameHistorys)
        {
            try
            {
                await _service
                    .ExecuteAsync(
                    command: new HandleRecordCommand(Record: gameHistory),
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                externalRoundIds.Add(gameHistory.ExternalRoundId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), gameHistory);
            }
        }

        if (externalRoundIds.Any())
        {
            _ = await _client
                .GameRecordMarkAsync(
                apiUrl: command.ApiUrl,
                appId: command.AppId,
                secretKey: command.SecretKey,
                externalRoundIds: externalRoundIds,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
