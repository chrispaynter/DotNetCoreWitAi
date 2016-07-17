using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paynter.WitAi.Models
{
    public class WitConverseResponse
    {
        public WitConverseType Type { get; set; }
        
        [JsonProperty("msg")]
        public string Message { get; set; }
        public string Action { get; set; }
        public JObject Entities { get; set; }

        public double Confidence { get; set; }
    }
}