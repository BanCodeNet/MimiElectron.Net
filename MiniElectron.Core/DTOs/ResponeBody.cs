using System.Text.Json.Serialization;

namespace MiniElectron.Core
{
    public sealed record ResponeBody
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("result")]
        public dynamic Result { get; set; }
    }
}