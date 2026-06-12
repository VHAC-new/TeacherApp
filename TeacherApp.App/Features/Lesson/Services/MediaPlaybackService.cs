using System.Net.Http.Json;
using TeacherApp.Contracts.Media;

namespace TeacherApp.App.Features.Lesson.Services;

public sealed class MediaPlaybackService(HttpClient http)
{
    /// <summary>Returns a local file:// URI that Plugin.Maui.Audio can play.</summary>
    public async Task<string?> ResolveLessonAudioUriAsync(Guid mediaId, CancellationToken ct = default)
    {
        var cachedPath = GetCachePath(mediaId, ".mp3");
        if (File.Exists(cachedPath))
            return new Uri(cachedPath).AbsoluteUri;

        var playbackResp = await http.GetAsync($"api/v1/media/{mediaId}/playback-url", ct);
        if (playbackResp.IsSuccessStatusCode)
        {
            var dto = await playbackResp.Content.ReadFromJsonAsync<MediaPlaybackUrlResponse>(cancellationToken: ct);
            if (dto?.Url is { Length: > 0 } url)
            {
                var ext = GuessExtensionFromUrl(url);
                var path = GetCachePath(mediaId, ext);

                using var downloadClient = new HttpClient();
                await using var input = await downloadClient.GetStreamAsync(url, ct);
                await using var output = File.Create(path);
                await input.CopyToAsync(output, ct);

                return new Uri(path).AbsoluteUri;
            }
        }

        var streamResp = await http.GetAsync($"api/v1/media/{mediaId}", ct);
        if (!streamResp.IsSuccessStatusCode)
            return null;

        var streamExt = GuessExtension(streamResp.Content.Headers.ContentType?.MediaType);
        var streamPath = GetCachePath(mediaId, streamExt);
        await using (var input = await streamResp.Content.ReadAsStreamAsync(ct))
        await using (var output = File.Create(streamPath))
        {
            await input.CopyToAsync(output, ct);
        }

        return new Uri(streamPath).AbsoluteUri;
    }

    private static string GetCachePath(Guid mediaId, string ext) =>
        Path.Combine(FileSystem.CacheDirectory, $"lesson_audio_{mediaId:N}{ext}");

    private static string GuessExtensionFromUrl(string url)
    {
        try
        {
            var path = new Uri(url).AbsolutePath;
            var ext = Path.GetExtension(path);
            if (!string.IsNullOrEmpty(ext) && ext.Length <= 5)
                return ext;
        }
        catch { }
        return ".mp3";
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
