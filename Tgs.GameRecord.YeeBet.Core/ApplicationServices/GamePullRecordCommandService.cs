using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Tgs.GameRounds.Common;
using Tgs.GameRounds.Common.MessageModels;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;

internal class GamePullRecordCommandService : ICommandService<GamePullRecordCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ILogger<GamePullRecordCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly MongoSettingOptions _mongoSetting;
    private readonly ITransferUtensil _utensil;

    public GamePullRecordCommandService(
        ILogger<GamePullRecordCommandService> logger,
        IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
        IOptions<MongoSettingOptions> mongoSettingOptions,
        IMessageSender messageSender,
        ITransferUtensil utensil)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _mongoSetting = mongoSettingOptions.Value;
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask ExecuteAsync(GamePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        foreach (var record in command.Records)
        {
            bool success = true;
            var playerModel = await _utensil
                .GetPlayerAsync(
                    playerCode: record.PlayerCode,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (playerModel == null)
            {
                success = false;
            }

            Guid? gameProviderConfigurationId = default;
            if (playerModel != null)
            {
                gameProviderConfigurationId = await _utensil
                    .GetBrandGameProviderConfigurationIdAsync(
                        brandId: playerModel.BrandId,
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

            var externalClosedOn = (DateTimeOffset?)null;
            var clientStatus = GameRoundClientStatus.Open;
            if (record.IsVoid)
            {
                clientStatus = GameRoundClientStatus.Void;
            }
            else
            {
                if (record.IsClosed)
                {
                    externalClosedOn = record.ExternalClosedOn.ToDateTimeOffset();
                    clientStatus = record.WinLoss.ToDecimal().GetValueOrDefault(0) switch
                    {
                        > 0 => GameRoundClientStatus.Win,
                        < 0 => GameRoundClientStatus.Loss,
                        _ => GameRoundClientStatus.Draw
                    };
                }
            }

            var status = record.IsClosed || record.IsVoid ? GameRoundStatus.Closed : GameRoundStatus.Open;

            var message = new GameRecordMessage(
                ExternalRoundId: record.ExternalRoundId,
                GameProviderId: gameProviderId,
                GameProviderCode: _gameRecordInfo.GameProviderCode,
                GameId: gameId,
                ExternalGameId: record.ExternalGameId,
                PlayerCode: (playerModel == null) ? string.Empty : playerModel.PlayerCode,
                CurrencyCode: (playerModel == null) ? string.Empty : playerModel.DefaultWalletCurrencyCode,
                WalletId: (playerModel == null) ? Guid.Empty : playerModel.DefaultWalletId,
                BrandId: (playerModel == null) ? Guid.Empty : playerModel.BrandId,
                GameProviderConfigurationId: gameProviderConfigurationId ?? Guid.Empty,
                Status: status,
                ClientStatus: clientStatus,
                ExternalCreatedOn: record.ExternalCreatedOn.ToDateTimeOffset(),
                ExternalClosedOn: externalClosedOn,
                BetAmount: record.Bet.ToDecimal().GetValueOrDefault(0),
                ValidBetAmount: record.ValidBet.ToDecimal().GetValueOrDefault(0),
                SettleAmount: record.Settle.ToDecimal().GetValueOrDefault(0),
                Commission: 0,
                Turnover: record.Turnover.ToDecimal().GetValueOrDefault(0),
                PlayerWinLoss: record.WinLoss.ToDecimal().GetValueOrDefault(0),
                AdditionalData: record.AdditionalData,
                Success: success);

            _logger.LogInformation("LogOn:{logOn}  Subject:{subject} MessageRequest:{message}",
                DateTime.Now.ToString("o"),
                _mongoSetting.GlobPattern,
                message);

            await _messageSender
                .PublishAsync(
                    subject: _mongoSetting.GlobPattern,
                    data: message.ToBson(),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
