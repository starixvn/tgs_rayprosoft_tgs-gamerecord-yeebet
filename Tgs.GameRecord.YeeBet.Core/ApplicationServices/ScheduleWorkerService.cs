using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.YeeBet.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;

internal class ScheduleWorkerService : ICommandService<ScheduleWorkerCommand>
{
    private readonly IGameInterfaceYeeBetClient _client;
    private readonly ILogger<ScheduleWorkerService> _logger;
    private readonly ICommandService<GamePullRecordCommand> _settledService;

    public ScheduleWorkerService(
        ILogger<ScheduleWorkerService> logger,
        IGameInterfaceYeeBetClient client,
        ICommandService<GamePullRecordCommand> settledService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _settledService = settledService ?? throw new ArgumentNullException(nameof(settledService));
    }

    public async ValueTask ExecuteAsync(ScheduleWorkerCommand command, CancellationToken cancellationToken = default)
    {
        try
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

            await _settledService
                .ExecuteAsync(
                    command: new GamePullRecordCommand(
                        Records: gameHistorys),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var externalRoundIds = gameHistorys.Select(x => x.ExternalRoundId).ToArray();

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "LogOn:{logOn} | Action:GameRecordAsync | Message:YeeBet 處理結算過程中發生例外", DateTime.UtcNow.ToString("O"));
        }
    }
}
