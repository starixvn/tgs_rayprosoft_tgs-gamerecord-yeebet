using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.YeeBet.Common;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.YeeBet.Controllers;

[Route("[controller]")]
[ApiController]
public class YeeBetController : ControllerBase
{
    /// <summary>
    /// Games the record manual asynchronous.
    /// </summary>
    /// <param name="messageSender">The message sender.</param>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Post
    ///         {
    ///             "apiUrl": "http://api.98kbus.top/",
    ///             "appId": "xtdA15OTVGEP",
    ///             "secretKey": "4885366DF30D880224FF6AA46FBDE4F0",
    ///             "startTime": "2023-04-10T02:39:08+08:00",
    ///             "endTime": "2023-04-10T02:39:08+08:00"
    ///         }
    /// </remarks>
    [HttpPost("manual")]
    public ValueTask GameRecordManualAsync(
        [FromServices] IMessageSender messageSender,
        YeeBetGameRecordManualMessage source)
        => messageSender.PublishAsync(
            subject: YeeBetNatsConst.ManualPublish,
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source)),
            cancellationToken: HttpContext.RequestAborted);

    /// <summary>
    /// Games the record schedule asynchronous.
    /// </summary>
    /// <param name="messageSender">The message sender.</param>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Post
    ///         {
    ///             "apiUrl": "http://api.98kbus.top/",
    ///             "appId": "xtdA15OTVGEP",
    ///             "secretKey": "4885366DF30D880224FF6AA46FBDE4F0"
    ///         }
    /// </remarks>
    [HttpPost("schedule")]
    public ValueTask GameRecordScheduleAsync(
        [FromServices] IMessageSender messageSender,
        YeeBetGameRecordScheduleMessage source)
        => messageSender.PublishAsync(
            subject: YeeBetNatsConst.SchedulePublish,
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source)),
            cancellationToken: HttpContext.RequestAborted);
}
