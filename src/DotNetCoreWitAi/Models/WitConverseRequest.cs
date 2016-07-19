using Newtonsoft.Json;

namespace Paynter.WitAi.Models
{
    public class WitConverseRequest
    {
        public WitConverseRequest()
        {}
        
        public WitConverseRequest(string sessionId, string query, dynamic context)
        {
            SessionId = sessionId;
            Query = query;
            Context = context;
        }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }
        [JsonProperty("q")]
        public string Query { get; set; }
        public dynamic Context { get; set; }
    }
}