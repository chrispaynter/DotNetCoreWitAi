using System.Collections.Generic;
using Newtonsoft.Json;

namespace Paynter.WitAi.Models
{
    public class WitEntity
    {
        public double Confidence { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class WitConverseResponse
    {
        public WitConverseType Type { get; set; }
        
        [JsonProperty("msg")]
        public string Message { get; set; }
        public string Action { get; set; }
        // public JObject Entities { get; set; }
        public Dictionary<string, List<WitEntity>> Entities { get; set; }

        public double Confidence { get; set; }

        public string GetFirstEntityValue(string entityName)
        {
            string val = null;

            if(Entities != null)
            {
                 var entities = Entities[entityName];

                 if(entities != null)
                 {
                     var subEntities = entities as List<WitEntity>;

                     if(subEntities != null && subEntities.Count > 0)
                     {
                         val = subEntities[0].Value;
                     }
                 }
            }

            return val;
        }
    }
}