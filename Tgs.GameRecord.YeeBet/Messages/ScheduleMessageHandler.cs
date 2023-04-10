using System.Text;
using System.Text.Json;
using Tgs.GameRecord.YeeBet.Core.DispatchServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.YeeBet.Messages;

internal class ScheduleMessageHandler : IMessageHandler
{
    private readonly ILogger<ScheduleMessageHandler> _logger;
    private readonly ICommandService<ScheduleDispatchCommand> _service;

    public ScheduleMessageHandler(
        ILogger<ScheduleMessageHandler> logger,
        ICommandService<ScheduleDispatchCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var message = JsonSerializer.Deserialize<ScheduleMessageModel>(json);

        if (message == null)
        {
            _logger.LogError("Information Received:{message}", message);
            return ValueTask.CompletedTask;
        }

        _logger.LogInformation("Information Received:{message}", message);

        var command = new ScheduleDispatchCommand(
            ApiUrl: message.ApiUrl,
            AppId: message.AppId,
            SecretKey: message.SecretKey);

        return _service.ExecuteAsync(
            command: command,
            cancellationToken: cancellationToken);
    }
}
