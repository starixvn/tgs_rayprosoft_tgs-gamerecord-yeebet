using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameCommand.Common;
using Tgs.GameInterface.YeeBet.Protos;
using Tgs.GameRecord.YeeBet.Core.ApplicationServices;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;

namespace Tgs.GameRecord.YeeBet.Core.UnitTest;

public class GamePullRecordCommandServiceUnitTest
{
    [Fact]
    public async Task Normal_Process()
    {
        var fakeOptions = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "YEEBET",
            GameProviderConfig = "YEEBETConfiguration",
            LockMinute = 10
        });

        var fakeLogger = NullLogger<GamePullRecordCommandService>.Instance;
        var fakeGameRoundsWorkClient = Substitute.For<IGameRoundsWorkClient>();
        var fakeUtensil = Substitute.For<IUtensil>();

        var command = new GamePullRecordCommand(
            Records: new GameRecordInfo[] {
            new GameRecordInfo()
            {
                PlayerCode =  "4ucmqplxb029",
                ExternalRoundId ="889026",
                IsVoid = false,
                IsClosed = true,
                ExternalCreatedOn = new DateTimeOffset(DateTime.Parse("2023-03-28 10:41:29"), TimeSpan.FromHours(+8)).ToTimestamp(),
                ExternalClosedOn = new DateTimeOffset(DateTime.Parse("2023-03-28 10:42:07"), TimeSpan.FromHours(+8)).ToTimestamp(),
                Bet = 60,
                ValidBet = 0,
                WinLoss =  0,
                Turnover = 0,
                Settle = 60,
                AdditionalData = string.Empty,
                ExternalGameId = "12"                
            }});

        var gameProviderId = Guid.NewGuid();
        _ = fakeUtensil
            .GetGameProviderIdAsync(
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var player = new PlayerInfo(
            ResellerId: Guid.NewGuid(),
            ResellerCode: "TGS",
            LicenseeId: Guid.NewGuid(),
            LicenseeCode: "TGS",
            BrandId: Guid.NewGuid(),
            BrandCode: "GPI",
            PlayerCode: "4ucmqplxb029",
            Username: "4ucmqplxb029",
            DefaultWalletId: Guid.NewGuid(),
            DefaultWalletCurrencyCode: "CNY");
        _ = fakeUtensil
            .GetPlayerAsync(
                playerCode: Arg.Is("4ucmqplxb029"),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(player);

        var gameId = 1;
        _ = fakeUtensil
            .GetGameIdAsync(
                Arg.Is(gameProviderId),
                Arg.Is(command.Records.Single().ExternalGameId),
                Arg.Any<CancellationToken>())
            .Returns(gameId);

        var service = new GamePullRecordCommandService(
            fakeLogger,
            fakeOptions,
            fakeGameRoundsWorkClient,
            fakeUtensil);

        var gameProviderCode = "YEEBET";
        var gameProviderConfigurationId = (Guid?)Guid.NewGuid();
        _ = fakeUtensil
            .GetBrandGameProviderConfigurationIdAsync(
                brandId: Arg.Is(player.BrandId),
                gameProviderId: Arg.Is(gameProviderId))
            .Returns(gameProviderConfigurationId);

        await service.ExecuteAsync(command).ConfigureAwait(false);

        await fakeGameRoundsWorkClient.Received(1)
             .RoundBidding(
                gameProviderId: Arg.Is(gameProviderId),
                gameProviderCode: Arg.Is(gameProviderCode),
                externalRoundId: Arg.Is(command.Records.First().ExternalRoundId),
                gameId: Arg.Is(gameId),
                externalGameId: Arg.Is(command.Records.First().ExternalGameId),
                playerCode: Arg.Is(player!.PlayerCode),
                walletId: Arg.Is(player!.DefaultWalletId),
                currencyCode: Arg.Is(player!.DefaultWalletCurrencyCode),
                brandId: Arg.Is(player!.BrandId),
                gameProviderConfigurationId: Arg.Is(gameProviderConfigurationId!.Value),
                status: Arg.Is(RoundStatus.Closed),
                clientStatus: Arg.Is(RoundClientStatus.Draw),
                externalCreatedOn: Arg.Is(command.Records.First().ExternalCreatedOn.ToDateTimeOffset()),
                externalClosedOn: Arg.Is(command.Records.First().ExternalClosedOn.ToDateTimeOffset()),
                bet: command.Records.First().Bet.ToDecimal().GetValueOrDefault(0),
                validBet: command.Records.First().ValidBet.ToDecimal().GetValueOrDefault(0),
                settle: command.Records.First().Settle.ToDecimal().GetValueOrDefault(0),
                commission: 0,
                turnover: command.Records.First().Turnover.ToDecimal().GetValueOrDefault(0),
                playerWinLoss: command.Records.First().WinLoss.ToDecimal().GetValueOrDefault(0),
                additionalData: command.Records.First().AdditionalData,
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }
}
