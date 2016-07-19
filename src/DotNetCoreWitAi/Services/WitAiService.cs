using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Paynter.WitAi.Models;
using Paynter.WitAi.Configuration;
using Paynter.WitAi.Exceptions;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Paynter.WitAi.Services
{
    // T1 - The request to converse
    // T2 - The response from converse (before the action was called)
    public class WitActionDictionary : Dictionary<string, Func<WitConverseRequest, WitConverseResponse, Task<dynamic>>>
    {
        public Func<WitConverseRequest, WitConverseResponse, Task<dynamic>> GetAction(string action)
        {
            return this.FirstOrDefault(u => u.Key.Equals(action, StringComparison.OrdinalIgnoreCase)).Value;
        }
    }

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

        public async Task<dynamic> RunActions(WitConverseRequest request, WitActionDictionary actions)
        {
            var response = await Converse(request);
            return await ContinueRunActions(request, response, actions);
        }

        public async Task<dynamic> ContinueRunActions(WitConverseRequest request, WitConverseResponse response, WitActionDictionary actions)
        {
            WitConverseRequest nextRequest;
            WitConverseResponse nextResponse;
            Func<WitConverseRequest, WitConverseResponse, Task<dynamic>> action;

            if(response.Type == WitConverseType.Unknown)
            {
                _logger.LogError("Missing response type: {@response}", response);
                throw new WitAiServiceException("Missing response type");
            }

            _logger.LogDebug("{@response}", response);

            // Ommitting backwards compatibility for 'merge' in API version 20160516

            switch(response.Type)
            {
                case WitConverseType.Error:
                    _logger.LogError("Wit API Error: {@response}", response);
                    throw new WitAiServiceException("WitAPI returned an error");

                case WitConverseType.Stop:
                    return request.Context;

                case WitConverseType.Message:
                    action = actions.GetAction("send");
                    await action?.Invoke(request, response);

                    nextRequest = new WitConverseRequest(request.SessionId, null, request.Context);
                    nextResponse = await Converse(nextRequest);
                    return await ContinueRunActions(nextRequest, nextResponse, actions);

                case WitConverseType.Action:
                    action = actions.GetAction(response.Action);

                    if(action == null)
                    {
                        _logger.LogError("Missing Action for {actionName}", response.Action);
                        throw new WitAiServiceException($"There is no action called {response.Action} in the passed in WitActionDictionary");
                    }

                    var newContext = await action?.Invoke(request, response);
                    newContext =  newContext == null ? new {} : newContext;

                    nextRequest = new WitConverseRequest(request.SessionId, null, newContext);
                    nextResponse = await Converse(nextRequest);
                    return await ContinueRunActions(nextRequest, nextResponse, actions);

                default:
                    // Unknown type
                    _logger.LogError("Unknown response type: {@response}", response);
                    throw new WitAiServiceException("Unknown response type");
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

            var response = await HttpClient.PostAsync($"/converse?{queryString}", requestString);
            
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
