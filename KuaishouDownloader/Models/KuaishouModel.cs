using Newtonsoft.Json;

namespace KuaishouDownloader.Models
{
    public class KuaishouModel
    {
        [JsonProperty("data")]
        public Data? Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("list")]
        public List<WorkItem>? List { get; set; }
    }

    public class WorkItem
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("poster")]
        public string? Poster { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("workType")]
        public string? WorkType { get; set; }
        [JsonProperty("liked")]
        public string? Liked { get; set; }
        [JsonProperty("author")]
        public Author? Author { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("useVideoPlayer")]
        public string? UseVideoPlayer { get; set; }
        [JsonProperty("imgUrls")]
        public List<string>? ImgUrls { get; set; }
        [JsonProperty("playUrl")]
        public string? PlayUrl { get; set; }
    }

    public class Author
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("avatar")]
        public string? Avatar { get; set; }
        [JsonProperty("playUrl")]
        public string? PlayUrl { get; set; }
        [JsonProperty("followStatus")]
        public string? FollowStatus { get; set; }
        [JsonProperty("constellation")]
        public string? Constellation { get; set; }
        [JsonProperty("cityName")]
        public string? CityName { get; set; }
        [JsonProperty("privacy")]
        public string? Privacy { get; set; }
        [JsonProperty("isNew")]
        public string? IsNew { get; set; }
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }
}
