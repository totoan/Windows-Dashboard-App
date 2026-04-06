using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

namespace DashboardApp;

public class YouTubeService
{
    private readonly HttpClient _client;

    public YouTubeService(string accessToken)
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<string> GetSubscriptionsAsync()
    {
        string url = "https://www.googleapis.com/youtube/v3/subscriptions" +
                     "?part=snippet" +
                     "&mine=true" +
                     "&maxResults=50";
        
        var response = await _client.GetAsync(url);
        
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> GetChannelDetailsAsync()
    {
        string json = await GetSubscriptionsAsync();
        var data = JsonSerializer.Deserialize<SubscriptionsResponse>(json);
        if (data == null)
            return "";

        List<string> channelIds = new List<string>();

        foreach (var item in data.items)
        {
            string channelId = item.snippet.resourceId.channelId;

            if (!string.IsNullOrWhiteSpace(channelId))
            {
                channelIds.Add(channelId);
            }
        }

        string joinedIds = string.Join(",", channelIds);
        string url = "https://www.googleapis.com/youtube/v3/channels" +
        "?part=contentDetails" +
        $"&id={joinedIds}";

        var response = await _client.GetAsync(url);

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<List<SubscriptionVideo>> GetUploadsAsync()
    {
        string json = await GetChannelDetailsAsync();
        var data = JsonSerializer.Deserialize<ChannelResponse>(json);
        
        List<string> playlistIds = new List<string>();

        if (data != null)
            foreach (var item in data.items)
            {
                string playlistId = item.contentDetails.relatedPlaylists.uploads;

                if (!string.IsNullOrWhiteSpace(playlistId))
                {
                    playlistIds.Add(playlistId);
                }
            }
        
        List<SubscriptionVideo> subVideos = new List<SubscriptionVideo>();
        
        foreach (string id in playlistIds)
        {
            string url = "https://www.googleapis.com/youtube/v3/playlistItems" +
                         "?part=snippet" +
                        $"&playlistId={id}" +
                         "&maxResults=5";
            
            var response = await _client.GetAsync(url);
            
            string playlistJson = await response.Content.ReadAsStringAsync();
            var playlistData = JsonSerializer.Deserialize<PlaylistItemsResponse>(playlistJson);

            if (playlistData != null)
                foreach (var item in playlistData.items)
                {
                    subVideos.Add(new SubscriptionVideo
                    {
                        Title = item.snippet.title,
                        ChannelTitle = item.snippet.channelTitle,
                        ThumbnailUrl = item.snippet.thumbnails.medium.url,
                        PublishedAt = item.snippet.publishedAt
                    });
                }
        }
        
        subVideos = subVideos.OrderByDescending(v => v.PublishedAt).ToList();
        subVideos = subVideos.Take(20).ToList();

        return subVideos;
    }
}