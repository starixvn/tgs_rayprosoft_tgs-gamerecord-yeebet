using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.YeeBet.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;

internal class ManualWorkerService : ICommandService<ManualWorkerCommand>
{
    private readonly IGameInterfaceYeeBetClient _client;
    private readonly ILogger<ManualWorkerService> _logger;
    private readonly ICommandService<ManualPullRecordCommand> _settledService;

    public ManualWorkerService(
        ILogger<ManualWorkerService> logger,
        IGameInterfaceYeeBetClient client,
        ICommandService<ManualPullRecordCommand> settledService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _settledService = settledService ?? throw new ArgumentNullException(nameof(settledService));
    }

    public async ValueTask ExecuteAsync(ManualWorkerCommand command, CancellationToken cancellationToken = default)
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

        await _settledService
            .ExecuteAsync(
                command: new ManualPullRecordCommand(
                    Records: reply.Items),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

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

            await _settledService
            .ExecuteAsync(
                command: new ManualPullRecordCommand(
                    Records: reply.Items),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        }
    }
}
