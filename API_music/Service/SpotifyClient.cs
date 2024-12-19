using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace API_music.Service;

public static class SpotifyClient
{
    private static readonly string ClientId = "CLIENT_ID";
    private static readonly string ClientSecret = "TOKEN";
    private static readonly string TokenUrl = "https://accounts.spotify.com/api/token";
    private static readonly string BaseUrl = "https://api.spotify.com/v1";

    private static async Task<string> GetAccessTokenAsync()
    {
        using (var client = new HttpClient())
        {
            var authenticationString = $"{ClientId}:{ClientSecret}";
            var base64EncodedAuthenticationString =
                Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
            });

            var response = await client.PostAsync(TokenUrl, requestBody);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            return json["access_token"].ToString();
        }
    }

    public static async Task<(string spotifyLink, string trackImageUrl)> SearchSongOnSpotifyAsync(string title,
        string artist)
    {
        var accessToken = await GetAccessTokenAsync();

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var query = Uri.EscapeDataString($"track:{title} artist:{artist}");
            var requestUrl = $"{BaseUrl}/search?q={query}&type=track&include_external=audio";

            var response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            var track = json["tracks"]["items"]?.FirstOrDefault();
            if (track == null)
            {
                return ("Track not found on Spotify.", null);
            }

            string spotifyLink = track["external_urls"]?["spotify"]?.ToString();
            string trackImageUrl = track["album"]?["images"]?.FirstOrDefault()?["url"]?.ToString();
            return (spotifyLink ?? "Spotify link not available.", trackImageUrl);
        }
    }
}