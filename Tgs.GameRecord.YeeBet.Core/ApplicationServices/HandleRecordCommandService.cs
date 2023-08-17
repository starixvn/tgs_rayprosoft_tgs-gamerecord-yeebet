using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;

internal class HandleRecordCommandService : ICommandService<HandleRecordCommand>
{
    private readonly IGameRecordHandleClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;

    public HandleRecordCommandService(
        IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
        IGameRecordHandleClient client)
    {
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var status = command.Record.IsClosed || command.Record.IsVoid ? GameRecordStatus.Closed : GameRecordStatus.Open;

        var externalClosedOn = (DateTimeOffset?)null;
        var clientStatus = GameRecordClientStatus.Open;
        if (command.Record.IsVoid)
        {
            clientStatus = GameRecordClientStatus.Void;
        }
        else
        {
            if (command.Record.IsClosed)
            {
                externalClosedOn = command.Record.ExternalClosedOn.ToDateTimeOffset();
                clientStatus = command.Record.WinLoss.ToDecimal().GetValueOrDefault(0) switch
                {
                    > 0 => GameRecordClientStatus.Win,
                    < 0 => GameRecordClientStatus.Loss,
                    _ => GameRecordClientStatus.Draw
                };
            }
        }

        await _client
            .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            externalRoundId: command.Record.ExternalRoundId,
            externalGameId: command.Record.ExternalGameId,
            playerCode: command.Record.PlayerCode,
            status: status,
            clientStatus: clientStatus,
            externalCreatedOn: command.Record.ExternalCreatedOn.ToDateTimeOffset(),
            externalClosedOn: externalClosedOn,
            bet: command.Record.Bet.ToDecimal().GetValueOrDefault(0),
            validBet: command.Record.ValidBet.ToDecimal().GetValueOrDefault(0),
            settle: command.Record.Settle.ToDecimal().GetValueOrDefault(0),
            commission: 0.0m,
            turnover: command.Record.Turnover.ToDecimal().GetValueOrDefault(0),
            playerWinLoss: command.Record.WinLoss.ToDecimal().GetValueOrDefault(0),
            additionalData: command.Record.AdditionalData,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
