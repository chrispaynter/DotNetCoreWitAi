using Newtonsoft.Json;

namespace Paynter.WitAi.Models
{
    public class WitConverseRequest
    {
        [JsonProperty("session_id")]
        public string SessionId { get; set; }
        [JsonProperty("q")]
        public string Query { get; set; }
        public object Context { get; set; }
    }
}