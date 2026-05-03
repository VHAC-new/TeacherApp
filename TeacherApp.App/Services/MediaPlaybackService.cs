using System.Net.Http.Json;
using TeacherApp.Contracts.Media;

namespace TeacherApp.App.Services;

public sealed class MediaPlaybackService(HttpClient http)
{
    /// <summary>Returns a playable URI string (https presigned or file:// cache path).</summary>
    public async Task<string?> ResolveLessonAudioUriAsync(Guid mediaId, CancellationToken ct = default)
    {
        var playbackResp = await http.GetAsync($"api/v1/media/{mediaId}/playback-url", ct);
        if (playbackResp.IsSuccessStatusCode)
        {
            var dto = await playbackResp.Content.ReadFromJsonAsync<MediaPlaybackUrlResponse>(cancellationToken: ct);
            if (dto?.Url is { Length: > 0 } url)
            {
                return url;
            }
        }

        var streamResp = await http.GetAsync($"api/v1/media/{mediaId}", ct);
        if (!streamResp.IsSuccessStatusCode)
        {
            return null;
        }

        var ext = GuessExtension(streamResp.Content.Headers.ContentType?.MediaType);
        var path = Path.Combine(FileSystem.CacheDirectory, $"lesson_audio_{mediaId:N}{ext}");
        await using (var input = await streamResp.Content.ReadAsStreamAsync(ct))
        await using (var output = File.Create(path))
        {
            await input.CopyToAsync(output, ct);
        }

        return new Uri(path).AbsoluteUri;
    }

    private static string GuessExtension(string? mediaType) => mediaType switch
    {
        "audio/mpeg" => ".mp3",
        "audio/wav" => ".wav",
        "audio/ogg" => ".ogg",
        "audio/mp4" or "audio/aac" => ".m4a",
        _ => ".bin"
    };
}
