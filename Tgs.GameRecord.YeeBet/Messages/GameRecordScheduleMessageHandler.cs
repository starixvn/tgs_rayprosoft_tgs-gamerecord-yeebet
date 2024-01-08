using System.Text;
using System.Text.Json;
using Tgs.GameRecord.YeeBet.Common;
using Tgs.GameRecord.YeeBet.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;
using Tgs.GameRecord.YeeBet.Messages;

namespace Tgs.GameRecord.YeeBet.Messages;

internal class GameRecordScheduleMessageHandler(
    ILogger<GameRecordScheduleMessageHandler> logger,
    ICommandService<SchedulePullRecordCommand> service) : IMessageHandler
{
    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var message = JsonSerializer.Deserialize<YeeBetGameRecordScheduleMessage>(json)
            ?? throw new NullReferenceException("The request data resolves to a null value");

        logger.LogInformation("LogOn:{logOn} Information Request:{message}", DateTime.Now.ToString("o"), message);

        var command = new SchedulePullRecordCommand(
            ApiUrl: message.ApiUrl,
            AppId: message.AppId,
            SecretKey: message.SecretKey);

        return service.ExecuteAsync(
            command: command,
            cancellationToken: cancellationToken);
    }
}
