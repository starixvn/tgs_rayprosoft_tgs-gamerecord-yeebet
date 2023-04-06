using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.YeeBet.Client;
using Tgs.GameRecord.YeeBet.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.WorkerServices;

internal class GameHistoryWorkerService : ICommandService<GameHistoryWorkerCommand>
{
    private readonly IGameInterfaceYeeBetClient _client;
    private readonly ILogger<GameHistoryWorkerService> _logger;
    private readonly ICommandService<GamePullRecordCommand> _settledService;

    public GameHistoryWorkerService(
        ILogger<GameHistoryWorkerService> logger,
        IGameInterfaceYeeBetClient client,
        ICommandService<GamePullRecordCommand> settledService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _settledService = settledService ?? throw new ArgumentNullException(nameof(settledService));
    }

    public async ValueTask ExecuteAsync(GameHistoryWorkerCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var gameHistorySettleds = await _client
                    .GameRecordAsync(
                        apiUrl: command.ApiUrl,
                        appId: command.AppId,
                        secretKey: command.SecretKey,
                        size: 1000,
                        cancellationToken: cancellationToken)
                    .ToArrayAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            _logger.LogInformation("LogOn:{logOn} | Action:GameRecordAsync | Response:{response}",
                DateTime.UtcNow.ToString("O"),
                JsonSerializer.Serialize(gameHistorySettleds));

            if (!gameHistorySettleds.Any())
            {
                return;
            }

            await _settledService
                .ExecuteAsync(
                    command: new GamePullRecordCommand(
                        Records: gameHistorySettleds),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var externalRoundIds = gameHistorySettleds.Select(x => x.ExternalRoundId).ToArray();

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
            _logger.LogError(ex, "LogOn:{logOn} | Action:GameHistorySettledAsync | Message:YeeBet 處理結算過程中發生例外", DateTime.UtcNow.ToString("O"));
        }
    }
}
