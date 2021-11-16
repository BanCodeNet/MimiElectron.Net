using System;
using System.Text.Json.Serialization;

namespace MiniElectron.Core
{
    public record CmdMessage
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; init; }
        [JsonPropertyName("topic")]
        public string Topic { get; init; }
        [JsonPropertyName("body")]
        public dynamic Body { get; init; }
        [JsonPropertyName("isCallback")]
        public bool IsCallback { get; init; }
    }
}