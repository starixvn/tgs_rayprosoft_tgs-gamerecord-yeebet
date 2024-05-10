using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;

internal class HandleRecordCommandService(
	ILogger<HandleRecordCommandService> logger,
	IOptions<GameRecordInfoOptions> options,
	IMessageSender messageSender) : ICommandService<HandleRecordCommand>
{
	private readonly GameRecordInfoOptions _gameRecordInfo = options.Value;

	public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
	{
		var betState = Handle.Common.Protos.BetState.Running;

		var externalClosedOn = DateTimeOffset.UnixEpoch;
		if (command.Record.IsVoid)
		{
			betState = Handle.Common.Protos.BetState.Void;
		}
		else
		{
			if (command.Record.IsClosed)
			{
				externalClosedOn = command.Record.ExternalClosedOn.ToDateTimeOffset();
				betState = command.Record.WinLoss.ToDecimal().GetValueOrDefault(0) switch
				{
					> 0 => Handle.Common.Protos.BetState.Win,
					< 0 => Handle.Common.Protos.BetState.Loss,
					_ => Handle.Common.Protos.BetState.Draw
				};
			}
		}

		var request = new Handle.Common.Protos.GameRecordHandleRequest
		{
			GameProviderCode = _gameRecordInfo.GameProviderCode,
			PlayerCode = command.Record.PlayerCode,
			ExternalRoundId = command.Record.ExternalRoundId,
			ExternalBetId = command.Record.ExternalRoundId,
			ExternalGameId = command.Record.ExternalGameId,
			ExternalCreatedOn = command.Record.ExternalCreatedOn,
			ExternalClosedOn = externalClosedOn.ToTimestamp(),
			ExternalPayoffOn = externalClosedOn.ToTimestamp(),
			ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
			BetState = betState,
			BetType = Handle.Common.Protos.BetType.Normal,
			FreeGameState = Handle.Common.Protos.FreeGameState.None,
			GambleState = Handle.Common.Protos.GambleState.None,
			CashoutState = Handle.Common.Protos.CashoutState.None,
			ResettleState = Handle.Common.Protos.ResettleState.None,
			JackpotState = Handle.Common.Protos.JackpotState.None,
			Bet = command.Record.Bet.ToDecimal().GetValueOrDefault(0),
			ValidBet = command.Record.ValidBet.ToDecimal().GetValueOrDefault(0),
			Settle = command.Record.Settle.ToDecimal().GetValueOrDefault(0),
			Commission = 0.0m,
			Turnover = command.Record.Turnover.ToDecimal().GetValueOrDefault(0),
			WinLoss = command.Record.WinLoss.ToDecimal().GetValueOrDefault(0),
			Jackpot = 0.0M,
			AdditionalData = command.Record.AdditionalData
		};

		await messageSender.PublishAsync(
			subject: Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest,
			data: request,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This publication requests information.({request})",
			DateTime.Now.ToString("o"),
			Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest,
			new
			{
				request.GameProviderCode,
				request.PlayerCode,
				request.ExternalRoundId,
				request.ExternalBetId,
				request.ExternalGameId,
				ExternalCreatedOn = request.ExternalCreatedOn.ToDateTimeOffset(),
				ExternalClosedOn = request.ExternalClosedOn.ToDateTimeOffset(),
				request.BetState
			});
	}
}
