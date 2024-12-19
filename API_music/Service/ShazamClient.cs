using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace API_music.Service;

public static class ShazamClient
{
    private static readonly string ApiKey = "TOKEN";
    private static readonly string ApiHost = "shazam.p.rapidapi.com";
    private static readonly string ApiUrl = "https://shazam.p.rapidapi.com/songs/v2/detect";

    public static async Task<string> RecognizeMusicAsync(FileInfo wavFile)
    {
        var base64Content = Convert.ToBase64String(await File.ReadAllBytesAsync(wavFile.FullName));
        var content = new StringContent(base64Content, Encoding.UTF8, "text/plain");

        using (var client = new HttpClient())
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiUrl),
                Headers =
                {
                    { "x-rapidapi-key", ApiKey },
                    { "x-rapidapi-host", ApiHost },
                },
                Content = content
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                // Parsing the JSON response
                var json = JObject.Parse(responseBody);

                var track = json["track"];
                if (track != null)
                {
                    string title = track["title"]?.ToString();
                    string subtitle = track["subtitle"]?.ToString();
                    JObject firstSection = track["sections"]?.First?["metadata"]?.First as JObject;
                    string album = firstSection?["text"]?.ToString();

                    return $"Title: {title}, Artist: {subtitle}, Album: {album}";
                }
                else
                {
                    return "Music not recognized.";
                }
            }
        }
    }
}