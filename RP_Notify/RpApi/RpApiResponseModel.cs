using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RP_Notify.RpApi.ResponseModel
{
    public class Info
    {

        [JsonPropertyName("song_id")]
        public string SongId { get; set; }

        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("album")]
        public string Album { get; set; }

        [JsonPropertyName("asin")]
        public string Asin { get; set; }

        [JsonPropertyName("avg_rating")]
        public double AvgRating { get; set; }

        [JsonPropertyName("num_ratings")]
        public string NumRatings { get; set; }

        [JsonPropertyName("ratings_dist")]
        public string RatingsDist { get; set; }

        [JsonPropertyName("user_rating")]
        public int UserRating { get; set; }

        [JsonPropertyName("web_link")]
        public string WebLink { get; set; }

        [JsonPropertyName("wiki_link")]
        public string WikiLink { get; set; }

        [JsonPropertyName("itunes_song_link")]
        public string ItunesSongLink { get; set; }

        [JsonPropertyName("itunes_album_link")]
        public string ItunesAlbumLink { get; set; }

        [JsonPropertyName("itunes_artist_link")]
        public string ItunesArtistLink { get; set; }

        [JsonPropertyName("facebook_link")]
        public string FacebookLink { get; set; }

        [JsonPropertyName("twitter_link")]
        public string TwitterLink { get; set; }

        [JsonPropertyName("rp_web_link")]
        public string RpWebLink { get; set; }

        [JsonPropertyName("amazon_cd_link")]
        public string AmazonCdLink { get; set; }

        [JsonPropertyName("amazon_mp3_link")]
        public string AmazonMp3Link { get; set; }

        [JsonPropertyName("amazon_search_link")]
        public string AmazonSearchLink { get; set; }

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; }

        [JsonPropertyName("length")]
        public string Length { get; set; }

        [JsonPropertyName("med_cover")]
        public string MedCover { get; set; }

        [JsonPropertyName("large_cover")]
        public string LargeCover { get; set; }

        [JsonPropertyName("lyrics_avail")]
        public string LyricsAvail { get; set; }

        [JsonPropertyName("lyrics")]
        public string Lyrics { get; set; }

        [JsonPropertyName("plays_30")]
        public int Plays30 { get; set; }

        [JsonPropertyName("slideshow")]
        public string Slideshow { get; set; }
    }


    public class NowplayingList
    {

        [JsonPropertyName("user_id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int UserId { get; set; }

        [JsonPropertyName("player_id")]
        public string PlayerId { get; set; }

        [JsonPropertyName("hist_num")]
        public int HistNum { get; set; }

        [JsonPropertyName("song")]
        public Dictionary<string, PlayListSong> Song { get; set; }

        [JsonPropertyName("refresh")]
        public int Refresh { get; set; }
    }


    public class PlayListSong
    {
        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonPropertyName("sched_time")]
        public string SchedTime { get; set; }

        [JsonPropertyName("song_id")]
        public string SongId { get; set; }

        [JsonPropertyName("chan")]
        public string Chan { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("album")]
        public string Album { get; set; }

        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("asin")]
        public string Asin { get; set; }

        [JsonPropertyName("rating")]
        public string Rating { get; set; }

        [JsonPropertyName("slideshow")]
        public string Slideshow { get; set; }

        [JsonPropertyName("user_rating")]
        public string UserRating { get; set; }

        [JsonPropertyName("cover")]
        public string Cover { get; set; }

        [JsonPropertyName("elapsed")]
        public int Elapsed { get; set; }
    }

    public class NowPlaying
    {

        [JsonPropertyName("time")]
        public int Time { get; set; }

        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("album")]
        public string Album { get; set; }

        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("cover")]
        public string Cover { get; set; }
    }

    public class Channel
    {

        [JsonPropertyName("chan")]
        public string Chan { get; set; }

        [JsonPropertyName("banner_url")]
        public string BannerUrl { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("stream_name")]
        public string StreamName { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }
    }

    public class GetBlock
    {

        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("end_event")]
        public string EndEvent { get; set; }

        [JsonPropertyName("length")]
        public string Length { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("chan")]
        public int Chan { get; set; }

        [JsonPropertyName("channel")]
        public Channel Channel { get; set; }

        [JsonPropertyName("bitrate")]
        public string Bitrate { get; set; }

        [JsonPropertyName("ext")]
        public string Ext { get; set; }

        [JsonPropertyName("cue")]
        public int Cue { get; set; }

        [JsonPropertyName("expiration")]
        public int Expiration { get; set; }

        [JsonPropertyName("filename")]
        public Dictionary<string, string> Filename { get; set; }

        [JsonPropertyName("image_base")]
        public string ImageBase { get; set; }

        [JsonPropertyName("song")]
        public Dictionary<string, PlayListSong> Song { get; set; }
    }

    public class Rating
    {

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("song_id")]
        public int SongId { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("rating")]
        public int UserRating { get; set; }
    }

    public class Auth
    {

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("post_ok")]
        public string PostOk { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("level")]
        public string Level { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("privmsg_new")]
        public bool PrivmsgNew { get; set; }

        [JsonPropertyName("passwd")]
        public string Passwd { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class Sync_v2
    {

        [JsonPropertyName("sync_id")]
        public string SyncId { get; set; }

        [JsonPropertyName("num_players")]
        public int NumPlayers { get; set; }

        [JsonPropertyName("players")]
        public IList<Player> Players { get; set; }

        [JsonPropertyName("channels")]
        public IList<Channel> Channels { get; set; }
    }

    public class Player
    {

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("player_id")]
        public string PlayerId { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("chan")]
        public string Chan { get; set; }
    }

}
