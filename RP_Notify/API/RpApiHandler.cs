﻿using RestSharp;
using RP_Notify.API.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RP_Notify.API
{
    class RpApiHandler : IRpApiHandler
    {
        private readonly RestClient _restClient;
        private readonly IConfig _config;
        private readonly ILogger _log;

        public RpApiHandler(IConfig config, ILog log, RestClient restClient)
        {
            _config = config;
            _restClient = restClient;
            _log = log.Logger;

            Init();
        }

        private void Init()
        {
            _log.Information("-- RpApiHandler - Initialization started - Checking for cookie cache and fetch RP channel list");

            _restClient.BaseUrl = new Uri(_config.InternalConfig.RpApiBaseUrl);

            ReadAndValidateCookieFromCache();

            _log.Information("-- RpApiHandler - Get channel list");
            _config.State.ChannelList = GetChannelList();
            _log.Information("-- RpApiHandler - Initialization finished - Channel list: {@ChannelList}", _config.State.ChannelList);
        }

        public NowplayingList GetNowplayingList(string channel = null, string player_id = null, int list_num = 1)
        {
            var request = new RestRequest("api/nowplaying_list", Method.GET);
            if (!string.IsNullOrEmpty(player_id))
            {
                request.AddParameter("player_id", player_id);
            }
            if (!string.IsNullOrEmpty(channel) && channel != "99")      // Exclude empty and Favourites
            {
                request.AddParameter("chan", channel);
            }
            request.AddParameter("list_num", list_num);

            var response = Task.Run(async () => await RestApiCallAsync<NowplayingList>(request)).Result;
            return response;
        }

        public NowPlaying GetNowPlaying(string channel = "0")
        {
            var request = new RestRequest("api/nowplaying", Method.GET);
            request.AddParameter("chan", channel);

            var response = Task.Run(async () => await RestApiCallAsync<NowPlaying>(request)).Result;
            return response;
        }

        public GetBlock GetGetBlock(string channel = "0", string info = "true", string bitrate = "4")
        {
            var request = new RestRequest("api/get_block", Method.GET);
            request.AddParameter("chan", channel);
            request.AddParameter("info", info);
            request.AddParameter("bitrate", bitrate);

            var response = Task.Run(async () => await RestApiCallAsync<GetBlock>(request)).Result;
            return response;
        }

        public Info GetInfo(string songId = null)
        {
            var request = new RestRequest("api/info", Method.GET);
            if (songId != null)
            {
                request.AddParameter("song_id", songId);
            }

            var response = Task.Run(async () => await RestApiCallAsync<Info>(request)).Result;
            return response;
        }

        public Rating GetRating(string songId, int rating)
        {
            var request = new RestRequest("api/rating", Method.GET);
            request.AddParameter("song_id", songId);
            request.AddParameter("rating", rating);

            var response = Task.Run(async () => await RestApiCallAsync<Rating>(request)).Result;
            return response;
        }

        public List<Channel> GetChannelList()
        {
            var request = new RestRequest("api/list_chan", Method.GET);

            var response = Task.Run(async () => await RestApiCallAsync<List<Channel>>(request)).Result;
            return response;
        }

        public Auth GetAuth(string username = null, string passwd = null)
        {
            var request = new RestRequest("api/auth", Method.GET);
            request.AddParameter("username", username);
            request.AddParameter("passwd", passwd);

            var response = Task.Run(async () => await RestApiCallAsync<Auth>(request)).Result;
            return response;
        }

        public Sync_v2 GetSync_v2()
        {
            var request = new RestRequest("api/sync_v2", Method.GET);
            request.AddParameter("mode", "track");

            var response = Task.Run(async () => await RestApiCallAsync<Sync_v2>(request)).Result;
            return response;
        }

        private Task<T> RestApiCallAsync<T>(RestRequest request) where T : new()
        {
            try
            {
                _log.Information("-- RestApiCallAsync invoked - URL resource path: {Resource} - Authenticated: {IsUserAuthenticated}", request.Resource, _config.State.IsUserAuthenticated);

                var taskCompletionSource = new TaskCompletionSource<T>();

                _restClient.ExecuteAsync<T>(request, (response) =>
                {
                    if (request.Resource.Contains("auth")) { RefreshCookieCache(response.Cookies); }
                    taskCompletionSource.SetResult(response.Data);
                });

                _log.Information("-- RestApiCallAsync returned - Result type: {ResultType}", taskCompletionSource.Task.Result.GetType());

                return taskCompletionSource.Task;
            }
            catch (Exception e)
            {
                _log.Error($"-- RestApiCallAsync - {e.Message}");
                return null;
            }
        }

        private void RefreshCookieCache(IList<RestResponseCookie> cookies)
        {
            if (cookies.Count > 0)
            {
                CookieContainer cookieJar = new CookieContainer();

                foreach (var cookie in cookies)
                {
                    cookieJar.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
                }

                _restClient.CookieContainer = cookieJar;
                _config.State.IsUserAuthenticated = true;

                if (CookieHelper.TryWriteCookieToDisk(_config.InternalConfig.CookieCachePath, cookieJar))
                {
                    _log.Information($"--RefreshCookieCache - Cookie saved to cache");
                }
                else
                {
                    _log.Error($"--RefreshCookieCache - Can't save cookie");
                }
            }
        }

        private void ReadAndValidateCookieFromCache()
        {
            if (CookieHelper.TryGetCookieFromCache(_config.InternalConfig.CookieCachePath, out var cookieCache))
            {
                _restClient.CookieContainer = cookieCache;
                _config.State.IsUserAuthenticated = true;

                if (GetAuth().Status == "success")
                {
                    _log.Information($"--ReadAndValidateCookieFromCache - Cookie validation Success");
                }
                else
                {
                    _config.State.IsUserAuthenticated = false;
                    Retry.Do(() => File.Delete(_config.InternalConfig.CookieCachePath));
                    _log.Warning($"--ReadAndValidateCookieFromCache - Invalid cookie found - DELETED");
                }
            }
            else
            {
                _config.State.IsUserAuthenticated = false;
                _log.Information($"--ReadAndValidateCookieFromCache - No cached cookie found");
            }
        }
    }
}
