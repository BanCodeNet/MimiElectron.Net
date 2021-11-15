using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mapster;

namespace MiniElectron.Core
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("dialog")]
    public sealed class DialogController : Controller
    {
        private readonly IpcBridge _ipcBridge;

        public DialogController(IpcBridge ipcBridge)
        {
            _ipcBridge = ipcBridge;
        }

        public sealed record ShowMessageBoxSyncRequest
        {
            [JsonPropertyName("message")]
            public string Message { get; init; }
            [JsonPropertyName("type")]
            public string Type { get; init; }
            [JsonPropertyName("buttons")]
            public string[] Buttons { get; init; }
            [JsonPropertyName("defaultId")]
            public int DefaultId { get; init; }
            [JsonPropertyName("title")]
            public string Title { get; init; }
            [JsonPropertyName("detail")]
            public string Detail { get; init; }
            [JsonPropertyName("icon")]
            public string Icon { get; init; }
            [JsonPropertyName("textWidth")]
            public int TextWidth { get; init; }
            [JsonPropertyName("cancelId")]
            public int CancelId { get; init; }
            [JsonPropertyName("noLink")]
            public bool NoLink { get; init; }
            [JsonPropertyName("normalizeAccessKeys")]
            public bool NormalizeAccessKeys { get; init; }
        }

        [HttpPost("showMessageBoxSync")]
        public async Task<dynamic> ShowMessageBoxSync(ShowMessageBoxSyncRequest request)
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<ShowMessageBoxSyncRequest, Dialog.ShowMessageBoxSyncOptions>().NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);
            var message = await _ipcBridge.DialogShowMessageBoxSync(request.Adapt<Dialog.ShowMessageBoxSyncOptions>(config));
            return message?.Body;
        }
    }
}