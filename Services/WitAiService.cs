using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Paynter.WitAi.Models;
using Paynter.WitAi.Configuration;
using Paynter.WitAi.Exceptions;

namespace Paynter.WitAi.Services
{
    public class WitAiService
    {
        private WitAiOptions _options;
        private ILogger<WitAiService> _logger;
        private HttpClient _httpClient;

        public WitAiService(IOptions<WitAiOptions> options, ILogger<WitAiService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public HttpClient HttpClient 
        { 
            get
            {
                if(_httpClient == null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.BaseAddress = new Uri(_options.ApiUrl);
                    _httpClient.DefaultRequestHeaders.Accept.Clear();
                    _httpClient.DefaultRequestHeaders.Add("Accept", $"application/vnd.wit.{_options.ApiVersion}+json");
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiToken}");
                }

                return _httpClient;
            }
        }

        public async Task<WitMessageResponse> Message(string message)
        {
            var queryString = $"q={WebUtility.UrlEncode(message)}";
            var response = await HttpClient.GetAsync($"/message?{queryString}");

            if(response.StatusCode != HttpStatusCode.OK)
            {
                var contents = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error sending message to Wit.AI Message API", contents, queryString, message);
                throw new WitAiServiceException("Error sending message to Wit.AI Message API", response, contents);
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WitMessageResponse>(content);
        }

        public async Task<WitConverseResponse> Converse(WitConverseRequest request)
        {
            if(string.IsNullOrEmpty(request.SessionId))
            {
                _logger.LogError("Called Converse API without mandatory SessionId");
                throw new WitAiServiceException("SessionId is mandatory");
            }

            var queryString = $"session_id={WebUtility.UrlEncode(request.SessionId)}";
            
            if(!string.IsNullOrEmpty(request.Query))
            {
                queryString = $"{queryString}&q={WebUtility.UrlEncode(request.Query)}";
            }

            StringContent requestString = new StringContent("");

            if(request.Context != null)
            {
                requestString = new StringContent(Serialise(request.Context), Encoding.UTF8, "application/json");
            }

            var response = await HttpClient.PostAsync($"/converse?{queryString}", new StringContent(""));
            
            if(response.StatusCode != HttpStatusCode.OK)
            {
                var contents = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error sending message to Wit.AI Converse API", contents, queryString, requestString);
                throw new WitAiServiceException("Error sending message to Wit.AI Converse API", response, contents);
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WitConverseResponse>(content);
        }

        private static string Serialise(object data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
