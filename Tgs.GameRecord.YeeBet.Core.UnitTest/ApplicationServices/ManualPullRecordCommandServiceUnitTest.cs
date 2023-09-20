using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameInterface.YeeBet.Client;
using Tgs.GameInterface.YeeBet.Protos;
using Tgs.GameRecord.YeeBet.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Core.UnitTest.ApplicationServices;

public class ManualPullRecordCommandServiceUnitTest
{
    [Fact]
    public async Task Normal_Process()
    {
        var fakeLogger = NullLogger<ManualPullRecordCommandService>.Instance;
        var fakeClient = Substitute.For<IGameInterfaceYeeBetClient>();
        var fakeService = Substitute.For<ICommandService<HandleRecordCommand>>();

        var command = new ManualPullRecordCommand(
            ApiUrl: new("http://api.98kbus.top/"),
            AppId: "xtdA15OTVGEP",
            SecretKey: "298511c51d785332d8d279998b79cd3c",
            BeginTime: DateTimeOffset.UtcNow.AddMinutes(-10),
            EndTime: DateTimeOffset.UtcNow);

        var reply = new GameRecordInfoReply()
        {
            ArraySize = 1,
            Total = 1,
        };

        reply.Items.Add(new GameRecordInfo()
        {
            PlayerCode = "4ucmqplxb029",
            ExternalRoundId = "889026",
            IsClosed = true,
            ExternalCreatedOn = new DateTimeOffset(DateTime.Parse("2023-03-28 10:41:29"), TimeSpan.FromHours(+8)).ToTimestamp(),
            ExternalClosedOn = new DateTimeOffset(DateTime.Parse("2023-03-28 10:42:07"), TimeSpan.FromHours(+8)).ToTimestamp(),
            Bet = 60m,
            ValidBet = 0m,
            WinLoss = 0.00m,
            Turnover = 0.00m,
            Settle = 60.0m,
            AdditionalData = string.Empty,
            IsVoid = false,
            ExternalGameId = "12"
        });

        _ = fakeClient
            .GameRecordInquireAsync(
                apiUrl: Arg.Is(command.ApiUrl),
                appId: Arg.Is(command.AppId),
                secretKey: Arg.Is(command.SecretKey),
                beginTime: Arg.Is(command.BeginTime),
                endTime: Arg.Is(command.EndTime),
                pageSize: 2000,
                pageIndex: 0)
            .Returns(reply);

        var sut = new ManualPullRecordCommandService(
            fakeLogger,
            fakeClient,
            fakeService);

        await sut.ExecuteAsync(command);

        await fakeService
            .Received(1)
            .ExecuteAsync(
                Arg.Any<HandleRecordCommand>());
    }
}
