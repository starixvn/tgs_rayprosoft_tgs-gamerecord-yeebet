using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;

internal class HandleRecordCommandService(
    IOptions<GameRecordInfoOptions> options,
    IGameRecordHandleClient client) : ICommandService<HandleRecordCommand>
{
    private readonly GameRecordInfoOptions _options = options.Value;

    public ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var wagerState = WagerStates.Running;

        var externalClosedOn = DateTimeOffset.UnixEpoch;
        if (command.Record.IsVoid)
        {
            wagerState = WagerStates.Void;
        }
        else
        {
            if (command.Record.IsClosed)
            {
                externalClosedOn = command.Record.ExternalClosedOn.ToDateTimeOffset();
                wagerState = command.Record.WinLoss.ToDecimal().GetValueOrDefault(0) switch
                {
                    > 0 => WagerStates.Win,
                    < 0 => WagerStates.Lose,
                    _ => WagerStates.Draw
                };
            }
        }

        return client
            .MessageAsync(
            gameProviderCode: _options.GameProviderCode,
            playerCode: command.Record.PlayerCode,
            wagerState: wagerState,
            wagerType: WagerTypes.Normal,
            freeGameState: FreeGameStates.None,
            gambleState: GambleStates.None,
            cashoutState: CashoutStates.None,
            resettleState: ResettleStates.None,
            externalRoundId: command.Record.ExternalRoundId,
            externalWagerId: command.Record.ExternalRoundId,
            externalGameId: command.Record.ExternalGameId,
            externalCreatedOn: command.Record.ExternalCreatedOn.ToDateTimeOffset(),
            externalClosedOn: externalClosedOn,
            externalPayoffOn: externalClosedOn,
            externalResettleOn: DateTimeOffset.UnixEpoch,
            bet: command.Record.Bet.ToDecimal().GetValueOrDefault(0),
            validBet: command.Record.ValidBet.ToDecimal().GetValueOrDefault(0),
            settle: command.Record.Settle.ToDecimal().GetValueOrDefault(0),
            commission: 0.0m,
            turnover: command.Record.Turnover.ToDecimal().GetValueOrDefault(0),
            winLoss: command.Record.WinLoss.ToDecimal().GetValueOrDefault(0),
            additionalData: JsonNode.Parse(command.Record.AdditionalData)!,
            cancellationToken: cancellationToken);
    }
}
