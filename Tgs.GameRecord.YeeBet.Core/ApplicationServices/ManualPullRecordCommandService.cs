using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameRounds.Work.Manual.Client;
using Tgs.GameRounds.Work.Manual.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;

internal class ManualPullRecordCommandService : ICommandService<ManualPullRecordCommand>
{
    private readonly IGameRoundsWorkManualClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ILogger<ManualPullRecordCommandService> _logger;
    private readonly IUtensil _utensil;

    public ManualPullRecordCommandService(
        ILogger<ManualPullRecordCommandService> logger,
        IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
        IGameRoundsWorkManualClient client,
        IUtensil utensil)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask ExecuteAsync(ManualPullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        foreach (var record in command.Records)
        {
            bool success = true;
            var player = await _utensil
                .GetPlayerAsync(
                    playerCode: record.PlayerCode,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (player == null)
            {
                success = false;
            }

            Guid? gameProviderConfigurationId = default;
            if (player != null)
            {
                gameProviderConfigurationId = await _utensil
                    .GetBrandGameProviderConfigurationIdAsync(
                        brandId: player.BrandId,
                        gameProviderId: gameProviderId,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                if (!gameProviderConfigurationId.HasValue)
                {
                    success = false;
                }
            }

            var gameId = await _utensil
                .GetGameIdAsync(
                    gameProviderId: gameProviderId,
                    gameExternalId: record.ExternalGameId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (success)
            {
                var status = record.IsClosed || record.IsVoid ? RoundStatus.Closed : RoundStatus.Open;

                var externalClosedOn = (DateTimeOffset?)null;
                var clientStatus = RoundClientStatus.Open;
                if (record.IsVoid)
                {
                    clientStatus = RoundClientStatus.Void;
                }
                else
                {
                    if (record.IsClosed)
                    {
                        externalClosedOn = record.ExternalClosedOn.ToDateTimeOffset();
                        clientStatus = record.WinLoss.ToDecimal().GetValueOrDefault(0) switch
                        {
                            > 0 => RoundClientStatus.Win,
                            < 0 => RoundClientStatus.Loss,
                            _ => RoundClientStatus.Draw
                        };
                    }
                }

                await _client.RoundBidding(
                    gameProviderId: gameProviderId,
                    gameProviderCode: _gameRecordInfo.GameProviderCode,
                    externalRoundId: record.ExternalRoundId,
                    gameId: gameId,
                    externalGameId: record.ExternalGameId,
                    playerCode: player!.PlayerCode,
                    walletId: player!.DefaultWalletId,
                    currencyCode: player!.DefaultWalletCurrencyCode,
                    brandId: player!.BrandId,
                    gameProviderConfigurationId: gameProviderConfigurationId!.Value,
                    status: status,
                    clientStatus: clientStatus,
                    externalCreatedOn: record.ExternalCreatedOn.ToDateTimeOffset(),
                    externalClosedOn: externalClosedOn,
                    bet: record.Bet.ToDecimal().GetValueOrDefault(0),
                    validBet: record.ValidBet.ToDecimal().GetValueOrDefault(0),
                    settle: record.Settle.ToDecimal().GetValueOrDefault(0),
                    commission: 0,
                    turnover: record.Turnover.ToDecimal().GetValueOrDefault(0),
                    playerWinLoss: record.WinLoss.ToDecimal().GetValueOrDefault(0),
                    additionalData: record.AdditionalData,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            }
            else
            {
                await _client.RoundMissing(
                    gameProviderId: gameProviderId,
                    gameProviderCode: _gameRecordInfo.GameProviderCode,
                    externalRoundId: record.ExternalRoundId,
                    gameId: gameId,
                    externalGameId: record.ExternalGameId,
                    playerCode: player?.PlayerCode ?? string.Empty,
                    data: record.AdditionalData,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            }
        }
    }
}
