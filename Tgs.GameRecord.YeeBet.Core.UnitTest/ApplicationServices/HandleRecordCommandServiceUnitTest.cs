using System.Text.Json.Nodes;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.YeeBet.Protos;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.YeeBet.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.YeeBet.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
	[Fact]
	public async Task Loss()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var fakeGameRecordInfoOptions = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "YEEBET",
			GameProviderConfiguration = "YEEBETConfiguration",
			HangfirePrefix = "gamerecord-yeebet-hangfire:",
			HangfireLocking = "gamerecord-yeebet-locking:",
			ScheduleLockingPrefix = "gamerecord-yeebet-schedule-locking:",
			ScheduleLockingMinute = 10
		});
		var fakePolicySettingsOptions = Options.Create(new PolicySettingsOptions
		{
			RetryCount = 1,
			RetrySeconds = 1,
		});
		var fakeGameRecordHandleClient = Substitute.For<IGameRecordHandleClient>();

		var command = new HandleRecordCommand(
			Record: new GameRecordInfo()
			{
				PlayerCode = "4ucmqplxb029",
				ExternalRoundId = "73272596",
				IsVoid = false,
				IsClosed = true,
				ExternalCreatedOn = new DateTimeOffset(DateTime.Parse("2024-05-10T13:19:47+08:00")).ToTimestamp(),
				ExternalClosedOn = new DateTimeOffset(DateTime.Parse("2024-05-10T13:20:02+08:00")).ToTimestamp(),
				Bet = 300.000000m,
				ValidBet = 300.000000m,
				WinLoss = -300.000000m,
				Turnover = 300.000000m,
				Settle = 0.000000m,
				AdditionalData = "{\"id\": 73272596, \"cid\": 1565660, \"uid\": \"xtC7L3M\", \"state\": 1, \"gameid\": 3014, \"gameno\": 1879858, \"userid\": 2801598, \"betodds\": 2.0, \"bettype\": 0, \"winlost\": -300.0, \"betpoint\": \"PLAYER\", \"currency\": \"PHP\", \"describe\": \"2801598:1565660:1407015:1\", \"gametype\": 4, \"username\": \"4uj9j0hjp1uj\", \"betamount\": 300.0, \"commamount\": 300.0, \"createtime\": 1715318387, \"gameresult\": \"3;2;6;11;45;0\", \"settletime\": 1715318402, \"userstatus\": 0, \"gameroundid\": 60435913, \"serialnumber\": 73272596}",
				ExternalGameId = "3014"
			});

		var sut = new HandleRecordCommandService(
			faceLogger,
			fakeGameRecordInfoOptions,
			fakePolicySettingsOptions,
			fakeGameRecordHandleClient);

		await sut.ExecuteAsync(command);

		_ = fakeGameRecordHandleClient
			.Received(1)
			.HandleAsync(
			source: Arg.Is(new Handle.Common.ViewModels.GameRecordHandleViewModel
			{
				GameProviderCode = fakeGameRecordInfoOptions.Value.GameProviderCode,
				PlayerCode = command.Record.PlayerCode,
				ExternalRoundId = command.Record.ExternalRoundId,
				ExternalBetId = command.Record.ExternalRoundId,
				ExternalGameId = command.Record.ExternalGameId,
				ExternalCreatedOn = command.Record.ExternalCreatedOn.ToDateTimeOffset(),
				ExternalClosedOn = command.Record.ExternalClosedOn.ToDateTimeOffset(),
				ExternalPayoffOn = command.Record.ExternalClosedOn.ToDateTimeOffset(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch,
				BetState = Handle.Common.Protos.BetState.Loss,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 300.000000m,
				ValidBet = 300.000000m,
				Settle = 0.000000m,
				Commission = 0.0M,
				Turnover = 300.000000m,
				WinLoss = -300.000000m,
				Jackpot = 0.0M,
				AdditionalData = command.Record.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Win()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var fakeGameRecordInfoOptions = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "YEEBET",
			GameProviderConfiguration = "YEEBETConfiguration",
			HangfirePrefix = "gamerecord-yeebet-hangfire:",
			HangfireLocking = "gamerecord-yeebet-locking:",
			ScheduleLockingPrefix = "gamerecord-yeebet-schedule-locking:",
			ScheduleLockingMinute = 10
		});
		var fakePolicySettingsOptions = Options.Create(new PolicySettingsOptions
		{
			RetryCount = 1,
			RetrySeconds = 1,
		});
		var fakeGameRecordHandleClient = Substitute.For<IGameRecordHandleClient>();

		var command = new HandleRecordCommand(
			Record: new GameRecordInfo()
			{
				PlayerCode = "4ucmqplxb029",
				ExternalRoundId = "73289288",
				IsVoid = false,
				IsClosed = true,
				ExternalCreatedOn = new DateTimeOffset(DateTime.Parse("2024-05-10T15:10:40+08:00")).ToTimestamp(),
				ExternalClosedOn = new DateTimeOffset(DateTime.Parse("2024-05-10T15:10:51+08:00")).ToTimestamp(),
				Bet = 50.000000m,
				ValidBet = 50.000000m,
				WinLoss = 50.000000m,
				Turnover = 50.000000m,
				Settle = 100.000000m,
				AdditionalData = "{\"id\": 73289288, \"cid\": 1565660, \"uid\": \"xtC7L3M\", \"state\": 1, \"gameid\": 3035, \"gameno\": 17337249, \"userid\": 2801598, \"betodds\": 2.0, \"bettype\": 0, \"winlost\": 50.0, \"betpoint\": \"PLAYER\", \"currency\": \"PHP\", \"describe\": \"2801598:1565660:1407015:1\", \"gametype\": 4, \"username\": \"4uj9j0hjp1uj\", \"betamount\": 50.0, \"commamount\": 50.0, \"createtime\": 1715325040, \"gameresult\": \"32;35;24;18;20;41\", \"settletime\": 1715325051, \"userstatus\": 0, \"gameroundid\": 60439814, \"serialnumber\": 73289288}",
				ExternalGameId = "3035"
			});

		var sut = new HandleRecordCommandService(
			faceLogger,
			fakeGameRecordInfoOptions,
			fakePolicySettingsOptions,
			fakeGameRecordHandleClient);

		await sut.ExecuteAsync(command);

		_ = fakeGameRecordHandleClient
			.Received(1)
			.HandleAsync(
			source: Arg.Is(new Handle.Common.ViewModels.GameRecordHandleViewModel
			{
				GameProviderCode = fakeGameRecordInfoOptions.Value.GameProviderCode,
				PlayerCode = command.Record.PlayerCode,
				ExternalRoundId = command.Record.ExternalRoundId,
				ExternalBetId = command.Record.ExternalRoundId,
				ExternalGameId = command.Record.ExternalGameId,
				ExternalCreatedOn = command.Record.ExternalCreatedOn.ToDateTimeOffset(),
				ExternalClosedOn = command.Record.ExternalClosedOn.ToDateTimeOffset(),
				ExternalPayoffOn = command.Record.ExternalClosedOn.ToDateTimeOffset(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch,
				BetState = Handle.Common.Protos.BetState.Win,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 50.000000m,
				ValidBet = 50.000000m,
				Settle = 100.000000m,
				Commission = 0.0M,
				Turnover = 50.000000m,
				WinLoss = 50.000000m,
				Jackpot = 0.0M,
				AdditionalData = command.Record.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Draw()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var fakeGameRecordInfoOptions = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "YEEBET",
			GameProviderConfiguration = "YEEBETConfiguration",
			HangfirePrefix = "gamerecord-yeebet-hangfire:",
			HangfireLocking = "gamerecord-yeebet-locking:",
			ScheduleLockingPrefix = "gamerecord-yeebet-schedule-locking:",
			ScheduleLockingMinute = 10
		});
		var fakePolicySettingsOptions = Options.Create(new PolicySettingsOptions
		{
			RetryCount = 1,
			RetrySeconds = 1,
		});
		var fakeGameRecordHandleClient = Substitute.For<IGameRecordHandleClient>();

		var command = new HandleRecordCommand(
			Record: new GameRecordInfo()
			{
				PlayerCode = "4ucmqplxb029",
				ExternalRoundId = "73282701",
				IsVoid = false,
				IsClosed = true,
				ExternalCreatedOn = new DateTimeOffset(DateTime.Parse("2024-05-10T15:10:40+08:00")).ToTimestamp(),
				ExternalClosedOn = new DateTimeOffset(DateTime.Parse("2024-05-10T15:10:51+08:00")).ToTimestamp(),
				Bet = 100.000000m,
				ValidBet = 0.000000m,
				WinLoss = 0.000000m,
				Turnover = 0.000000m,
				Settle = 100.000000m,
				AdditionalData = "{\"id\": 73282701, \"cid\": 1565660, \"uid\": \"xtC7L3M\", \"state\": 1, \"gameid\": 3012, \"gameno\": 1968053, \"userid\": 2801598, \"betodds\": 1.0, \"bettype\": 0, \"winlost\": 0.0, \"betpoint\": \"PLAYER\", \"currency\": \"PHP\", \"describe\": \"2801598:1565660:1407015:1\", \"gametype\": 4, \"username\": \"4uj9j0hjp1uj\", \"betamount\": 100.0, \"commamount\": 0.0, \"createtime\": 1715322055, \"gameresult\": \"51;35;49;1;45;42\", \"settletime\": 1715322086, \"userstatus\": 0, \"gameroundid\": 60438069, \"serialnumber\": 73282701}",
				ExternalGameId = "3012"
			});

		var sut = new HandleRecordCommandService(
			faceLogger,
			fakeGameRecordInfoOptions,
			fakePolicySettingsOptions,
			fakeGameRecordHandleClient);

		await sut.ExecuteAsync(command);

		_ = fakeGameRecordHandleClient
			.Received(1)
			.HandleAsync(
			source: Arg.Is(new Handle.Common.ViewModels.GameRecordHandleViewModel
			{
				GameProviderCode = fakeGameRecordInfoOptions.Value.GameProviderCode,
				PlayerCode = command.Record.PlayerCode,
				ExternalRoundId = command.Record.ExternalRoundId,
				ExternalBetId = command.Record.ExternalRoundId,
				ExternalGameId = command.Record.ExternalGameId,
				ExternalCreatedOn = command.Record.ExternalCreatedOn.ToDateTimeOffset(),
				ExternalClosedOn = command.Record.ExternalClosedOn.ToDateTimeOffset(),
				ExternalPayoffOn = command.Record.ExternalClosedOn.ToDateTimeOffset(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch,
				BetState = Handle.Common.Protos.BetState.Draw,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 100.000000m,
				ValidBet = 0.000000m,
				Settle = 100.000000m,
				Commission = 0.000000m,
				Turnover = 0.000000m,
				WinLoss = 0.000000m,
				Jackpot = 0.000000m,
				AdditionalData = command.Record.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
