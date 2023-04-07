using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameInterface.YeeBet.Client;
using Tgs.GameInterface.YeeBet.Protos;
using Tgs.GameRecord.YeeBet.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.UnitTest;

public class ScheduleWorkerServiceUnitTest
{
    [Fact]
    public async Task Normal_Process()
    {
        var fakeLogger = NullLogger<ScheduleWorkerService>.Instance;
        var fakeClient = Substitute.For<IGameInterfaceYeeBetClient>();
        var fakeService = Substitute.For<ICommandService<GamePullRecordCommand>>();

        var command = new ScheduleWorkerCommand(
            ApiUrl: new("http://api.98kbus.top/"),
            AppId: "xtdA15OTVGEP",
            SecretKey: "298511c51d785332d8d279998b79cd3c");

        var gameRecords = new GameRecordInfo[] {
            new GameRecordInfo()
            {
                PlayerCode =  "4ucmqplxb029",
                ExternalRoundId ="889026",
                IsClosed = true,
                ExternalCreatedOn = new DateTimeOffset(DateTime.Parse("2023-03-28 10:41:29"), TimeSpan.FromHours(+8)).ToTimestamp(),
                ExternalClosedOn = new DateTimeOffset(DateTime.Parse("2023-03-28 10:42:07"), TimeSpan.FromHours(+8)).ToTimestamp(),
                Bet = 60m,
                ValidBet = 0m,
                WinLoss =  0.00m,
                Turnover = 0.00m,
                Settle = 60.0m,
                AdditionalData = string.Empty,
                IsVoid = false,
                ExternalGameId = "12"
            }};

        _ = fakeClient
            .GameRecordAsync(
                Arg.Is(command.ApiUrl),
                Arg.Is(command.AppId),
                Arg.Is(command.SecretKey),
                2000
            ).Returns(gameRecords.ToAsyncEnumerable());

        var sut = new ScheduleWorkerService(
            fakeLogger,
            fakeClient,
            fakeService);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        await fakeService
            .Received(1)
            .ExecuteAsync(
                Arg.Any<GamePullRecordCommand>())
            .ConfigureAwait(false);

        _ = await fakeClient
            .Received(1)
            .GameRecordMarkAsync(
                apiUrl: Arg.Is(command.ApiUrl),
                appId: Arg.Is(command.AppId),
                secretKey: Arg.Is(command.SecretKey),
                externalRoundIds: Arg.Any<string[]>())
             .ConfigureAwait(false);
    }
}
