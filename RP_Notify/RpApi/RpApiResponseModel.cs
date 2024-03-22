using Newtonsoft.Json;
using System.Collections.Generic;

namespace RP_Notify.RpApi.ResponseModel
{
    public class Info
    {

        [JsonProperty("song_id")]
        public string SongId { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("album")]
        public string Album { get; set; }

        [JsonProperty("asin")]
        public string Asin { get; set; }

        [JsonProperty("avg_rating")]
        public double AvgRating { get; set; }

        [JsonProperty("num_ratings")]
        public string NumRatings { get; set; }

        [JsonProperty("ratings_dist")]
        public string RatingsDist { get; set; }

        [JsonProperty("user_rating")]
        public int UserRating { get; set; }

        [JsonProperty("web_link")]
        public string WebLink { get; set; }

        [JsonProperty("wiki_link")]
        public string WikiLink { get; set; }

        [JsonProperty("itunes_song_link")]
        public string ItunesSongLink { get; set; }

        [JsonProperty("itunes_album_link")]
        public string ItunesAlbumLink { get; set; }

        [JsonProperty("itunes_artist_link")]
        public string ItunesArtistLink { get; set; }

        [JsonProperty("facebook_link")]
        public string FacebookLink { get; set; }

        [JsonProperty("twitter_link")]
        public string TwitterLink { get; set; }

        [JsonProperty("rp_web_link")]
        public string RpWebLink { get; set; }

        [JsonProperty("amazon_cd_link")]
        public string AmazonCdLink { get; set; }

        [JsonProperty("amazon_mp3_link")]
        public string AmazonMp3Link { get; set; }

        [JsonProperty("amazon_search_link")]
        public string AmazonSearchLink { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }

        [JsonProperty("med_cover")]
        public string MedCover { get; set; }

        [JsonProperty("large_cover")]
        public string LargeCover { get; set; }

        [JsonProperty("lyrics_avail")]
        public string LyricsAvail { get; set; }

        [JsonProperty("lyrics")]
        public string Lyrics { get; set; }

        [JsonProperty("plays_30")]
        public int Plays30 { get; set; }

        [JsonProperty("slideshow")]
        public string Slideshow { get; set; }
    }


    public class NowplayingList
    {

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("player_id")]
        public string PlayerId { get; set; }

        [JsonProperty("hist_num")]
        public int HistNum { get; set; }

        [JsonProperty("song")]
        public Dictionary<string, PlayListSong> Song { get; set; }

        [JsonProperty("refresh")]
        public int Refresh { get; set; }
    }


    public class PlayListSong
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("sched_time")]
        public string SchedTime { get; set; }

        [JsonProperty("song_id")]
        public string SongId { get; set; }

        [JsonProperty("chan")]
        public string Chan { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("album")]
        public string Album { get; set; }

        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("asin")]
        public string Asin { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("slideshow")]
        public string Slideshow { get; set; }

        [JsonProperty("user_rating")]
        public string UserRating { get; set; }

        [JsonProperty("cover")]
        public string Cover { get; set; }

        [JsonProperty("elapsed")]
        public int Elapsed { get; set; }
    }

    public class NowPlaying
    {

        [JsonProperty("time")]
        public int Time { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("album")]
        public string Album { get; set; }

        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("cover")]
        public string Cover { get; set; }
    }

    public class Channel
    {

        [JsonProperty("chan")]
        public string Chan { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("stream_name")]
        public string StreamName { get; set; }

        [JsonProperty("player_id")]
        public string PlayerId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class GetBlock
    {

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("end_event")]
        public string EndEvent { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("chan")]
        public int Chan { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }

        [JsonProperty("bitrate")]
        public string Bitrate { get; set; }

        [JsonProperty("ext")]
        public string Ext { get; set; }

        [JsonProperty("cue")]
        public int Cue { get; set; }

        [JsonProperty("expiration")]
        public int Expiration { get; set; }

        [JsonProperty("filename")]
        public Dictionary<string, string> Filename { get; set; }

        [JsonProperty("image_base")]
        public string ImageBase { get; set; }

        [JsonProperty("song")]
        public Dictionary<string, PlayListSong> Song { get; set; }
    }

    public class Rating
    {

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("song_id")]
        public int SongId { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("rating")]
        public int UserRating { get; set; }
    }

    public class Auth
    {

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("post_ok")]
        public string PostOk { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("privmsg_new")]
        public bool PrivmsgNew { get; set; }

        [JsonProperty("passwd")]
        public string Passwd { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class Sync_v2
    {

        [JsonProperty("sync_id")]
        public string SyncId { get; set; }

        [JsonProperty("num_players")]
        public int NumPlayers { get; set; }

        [JsonProperty("players")]
        public IList<Player> Players { get; set; }

        [JsonProperty("channels")]
        public IList<Channel> Channels { get; set; }
    }

    public class Player
    {

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("player_id")]
        public string PlayerId { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("chan")]
        public string Chan { get; set; }
    }

}
