using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Paynter.WitAi.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WitConverseType
    {
        Merge,
        [EnumMember(Value = "msg")]
        Message,
        Action,
        Stop
    }
}