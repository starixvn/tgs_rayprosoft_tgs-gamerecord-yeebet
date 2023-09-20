using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.YeeBet.Core.ApplicationServices;
using Tgs.GameRecord.YeeBet.Messages;
using Tgs.GameRecord.YeeBet.ViewModels;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.YeeBet.Controllers;

[Route("[controller]")]
[ApiController]
public class YeeBetController : ControllerBase
{
    /// <summary>
    /// Manuals the asynchronous.
    /// </summary>
    /// <param name="sender">The sender.</param>
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
    ///             "beginTime": "2023-04-10T02:39:08+08:00",
    ///             "endTime": "2023-04-10T02:39:08+08:00"
    ///         }
    /// </remarks>
    [HttpPost("Manual")]
    public async ValueTask ManualAsync(
        [FromServices] IMessageSender sender,
        [FromBody] ManualQueryViewModel source)
    {
        var request = new ManualMessageModel(
            ApiUrl: source.ApiUrl,
            AppId: source.AppId,
            SecretKey: source.SecretKey,
            BeginTime: source.BeginTime,
            EndTime: source.EndTime);

        _ = await sender.RequestAsync(
                subject: "gp.gamerecord.manual.yeebet",
                data: JsonSerializer.SerializeToUtf8Bytes(request),
                cancellationToken: HttpContext.RequestAborted)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Manuals the exectue asynchronous.
    /// </summary>
    /// <param name="service">The service.</param>
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
    ///             "beginTime": "2023-04-10T02:39:08+08:00",
    ///             "endTime": "2023-04-10T02:39:08+08:00"
    ///         }
    /// </remarks>
    [HttpPost]
    public async ValueTask ManualExecueAsync(
        [FromServices] ICommandService<ManualPullRecordCommand> service,
        [FromBody] ManualQueryViewModel source)
    {
        await service.ExecuteAsync(
            command: new ManualPullRecordCommand(
                source.ApiUrl,
                source.AppId,
                source.SecretKey,
                source.BeginTime,
                source.EndTime),
            cancellationToken: HttpContext.RequestAborted)
            .ConfigureAwait(false);
    }
}
