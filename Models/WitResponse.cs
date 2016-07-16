using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paynter.WitAi
{
    public class WitResponse
    {
        [JsonProperty("msg_id")]
        public string MessageId { get; set; }
        
        [JsonProperty("_text")]
        public string Text { get; set; }

        public JObject Entities { get; set; }
    }
}