using RP_Notify.Config;
using RP_Notify.Helpers;
using RP_Notify.Logger;
using RP_Notify.RpApi.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RP_Notify.RpApi
{
    class RpApiClient : IRpApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigRoot _config;
        private readonly ILoggerWrapper _log;

        public RpApiClient(IConfigRoot config, ILoggerWrapper log, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _log = log;
            _httpClientFactory = httpClientFactory;
        }

        public NowplayingList GetNowplayingList(int list_num = 1)
        {
            var requestPath = "api/nowplaying_list";

            var parameters = new Dictionary<string, string>();
            parameters.Add("chan", _config.ExternalConfig.Channel.ToString());
            parameters.Add("list_num", list_num.ToString());
            if (!string.IsNullOrEmpty(_config.State.RpTrackingConfig.ActivePlayerId))
            {
                parameters.Add("player_id", _config.State.RpTrackingConfig.ActivePlayerId);
            }


            var response = RestApiCallAsync<NowplayingList>(requestPath, parameters, HttpMethod.Get).Result;
            return response;
        }

        public NowPlaying GetNowPlaying(string channel = "0")
        {
            var requestPath = "api/nowplaying";

            var parameters = new Dictionary<string, string>();
            parameters.Add("chan", channel);

            var response = RestApiCallAsync<NowPlaying>(requestPath, parameters, HttpMethod.Get).Result;

            return response;
        }

        public GetBlock GetGetBlock(string channel = "0", string info = "true", string bitrate = "4")
        {
            var requestPath = "api/get_block";

            var parameters = new Dictionary<string, string>();
            parameters.Add("chan", channel);
            parameters.Add("info", info);
            parameters.Add("bitrate", bitrate);

            var response = RestApiCallAsync<GetBlock>(requestPath, parameters, HttpMethod.Get).Result;

            return response;
        }

        public Info GetInfo(string songId = null)
        {
            var requestPath = "api/info";

            var parameters = new Dictionary<string, string>();
            parameters.Add("song_id", songId);

            var response = RestApiCallAsync<Info>(requestPath, parameters, HttpMethod.Get).Result;

            return response;
        }

        public Rating GetRating(string songId, int rating)
        {


            var requestPath = "api/rating";

            var parameters = new Dictionary<string, string>();
            parameters.Add("song_id", songId);
            parameters.Add("rating", rating.ToString());

            var response = RestApiCallAsync<Rating>(requestPath, parameters, HttpMethod.Get).Result;

            return response;
        }

        public List<Channel> GetChannelList()
        {
            var requestPath = "api/list_chan";

            var response = RestApiCallAsync<List<Channel>>(requestPath, null, HttpMethod.Get).Result;

            return response;
        }

        public Auth GetAuth(string username = null, string passwd = null)
        {
            var requestPath = "api/auth";

            var parameters = new Dictionary<string, string>();
            parameters.Add("username", username);
            parameters.Add("passwd", passwd);

            var response = RestApiCallAsync<Auth>(requestPath, parameters, HttpMethod.Get).Result;

            return response;
        }

        public Sync_v2 GetSync_v2()
        {
            var requestPath = "api/sync_v2";

            var parameters = new Dictionary<string, string>();
            parameters.Add("mode", "track");

            var response = RestApiCallAsync<Sync_v2>(requestPath, parameters, HttpMethod.Get).Result;

            return response;
        }

        private async Task<T> RestApiCallAsync<T>(string requestPath, Dictionary<string, string> parameters, HttpMethod method) where T : new()
        {
            try
            {
                _log.Information(LogHelper.GetMethodName(this), "Invoked - URL resource path: {Resource} - Authenticated: {IsUserAuthenticated}", requestPath, _config.IsUserAuthenticated());

                var rpBaseAddressUri = new Uri(_config.StaticConfig.RpApiBaseUrl);


                var requestFullPath = requestPath;

                if (parameters != null)
                {
                    var queryParamString = string
                        .Join("&", parameters
                            .Where(p => !string.IsNullOrEmpty(p.Key) && !string.IsNullOrEmpty(p.Value))
                            .Select(kvp => $"{kvp.Key}={kvp.Value}"));

                    requestFullPath = !string.IsNullOrEmpty(queryParamString)
                        ? requestFullPath + "?" + queryParamString
                        : requestFullPath;
                }

                var request = new HttpRequestMessage(method, requestFullPath);

                var client = _httpClientFactory.CreateClient(this.GetType().Name);
                client.BaseAddress = rpBaseAddressUri;

                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<T>(responseContent);

                    _log.Information(LogHelper.GetMethodName(this), "Returned - Result type: {ResultType}", result.GetType());

                    if (requestPath == "api/auth" && response.Headers.TryGetValues("Set-Cookie", out var responeCookies))
                    {
                        if (responeCookies.Any(c =>
                            c.Contains("C_username=anonymous")
                            || c.Contains("C_passwd=deleted")
                            || c.Contains("C_validated=deleted")
                            )
                        )
                        {
                            _log.Error(LogHelper.GetMethodName(this), "Authentication failed - the returned cookie is invalid");
                            return result;
                        }

                        var cookieContainer = new CookieContainer();

                        foreach (var cookie in responeCookies)
                        {
                            cookieContainer.SetCookies(rpBaseAddressUri, cookie);
                        }

                        _config.State.RpCookieContainer = cookieContainer;
                    }

                    return result;
                }
                else
                {
                    _log.Error(LogHelper.GetMethodName(this), "HTTP request failed - Status code: {StatusCode} - Reason: {Reason}", response.StatusCode, response.ReasonPhrase);
                    return default;
                }
            }
            catch (Exception ex)
            {
                _log.Error(LogHelper.GetMethodName(this), ex);
                throw;
            }
        }
    }
}
