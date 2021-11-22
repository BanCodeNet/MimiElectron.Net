using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mapster;

namespace MiniElectron.Core;

[ApiController]
[ApiVersion("1.0")]
[Route("notification")]
public sealed class NotificationController : Controller
{
    private readonly Bridge _bridge;

    public NotificationController(Bridge bridge)
    {
        _bridge = bridge;
    }

    [HttpGet("isSupported")]
    public async Task<dynamic> IsSupported()
    {
        var message = await _bridge.NotificationIsSupported();
        return message?.Body;
    }

    [HttpPost("show")]
    public Task Show(ShowRequest request)
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<ShowRequest, Notification.ShowOptions>().NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);
        return _bridge.NotificationShow(request.Adapt<Notification.ShowOptions>(config));
    }
}

public sealed record ShowRequest
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; }
    [JsonPropertyName("body")]
    public string Body { get; set; }
    [JsonPropertyName("silent")]
    public string Silent { get; set; }
    [JsonPropertyName("icon")]
    public string Icon { get; set; }
    [JsonPropertyName("hasReply")]
    public bool HasReply { get; set; }
    [JsonPropertyName("timeoutType")]
    public string TimeoutType { get; set; }
    [JsonPropertyName("replyPlaceholder")]
    public string ReplyPlaceholder { get; set; }
    [JsonPropertyName("sound")]
    public string Sound { get; set; }
    [JsonPropertyName("urgency")]
    public string Urgency { get; set; }
    [JsonPropertyName("closeButtonText")]
    public string CloseButtonText { get; set; }
    [JsonPropertyName("toastXml")]
    public string ToastXml { get; set; }
}