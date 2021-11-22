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
        private readonly Bridge _bridge;

        public DialogController(Bridge bridge)
        {
            _bridge = bridge;
        }

        public sealed record ShowMessageBoxRequest
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

        [HttpPost("showMessageBox")]
        public async Task<dynamic> ShowMessageBoxSync(ShowMessageBoxRequest request)
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<ShowMessageBoxRequest, Dialog.ShowMessageBoxOptions>().NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);
            var message = await _bridge.DialogShowMessageBox(request.Adapt<Dialog.ShowMessageBoxOptions>(config));
            return message?.Body;
        }
    }
}