using System;
using System.Net.Http;

namespace Paynter.WitAi.Exceptions
{
    public class WitAiServiceException:Exception
    {
        public HttpResponseMessage Response { get; private set; }
        public string ResponseContent { get; private set; }
        public WitAiServiceException(string message):base(message)
        {
        }

        public WitAiServiceException(string message, HttpResponseMessage response)
        {
            Response = response;
        }

        public WitAiServiceException(string message, HttpResponseMessage response, string responseContent)
        {
            Response = response;
            ResponseContent = responseContent;
        }
    }
}