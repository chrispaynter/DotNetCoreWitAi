using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paynter.WitAi
{
    public class WitOutcome
    {
        [JsonProperty("msg_id")]
        public int MessageId { get; set; }
        
        public string Intent { get; set; }

        public JObject Entities { get; set; }

        public double Confidence { get; set; }
    }
}