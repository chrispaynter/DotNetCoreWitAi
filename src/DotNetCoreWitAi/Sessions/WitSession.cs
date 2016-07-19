using System.Dynamic;

namespace Paynter.WitAi.Sessions
{
    public class WitSession
    {
        public WitSession(string facebookSenderId, string witSessionId)
        {
            UserId = facebookSenderId;
            WitSessionId = witSessionId;
            Context = new ExpandoObject();
        }
        public string UserId { get; set; }
        public string WitSessionId { get; set; }
        public dynamic Context { get; set; }
    }
}