using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeacherApp.Api.Data;
using TeacherApp.Api.Infrastructure.Storage;
using TeacherApp.Contracts.Media;

namespace TeacherApp.Api.Application.Media;

public sealed class MediaPlaybackService(
    AppDbContext db,
    IS3MediaOperations s3,
    IOptions<MediaS3Options> s3Options) : IMediaPlaybackService
{
    public async Task<MediaPlaybackUrlResponse?> GetPresignedPlaybackUrlAsync(Guid mediaId, CancellationToken cancellationToken)
    {
        if (!s3.IsEnabled)
            return null;

        var media = await db.MediaFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == mediaId, cancellationToken);

        if (media is null || !media.UploadCompleted)
            return null;

        var minutes = s3Options.Value.GetUrlExpirationMinutes > 0 ? s3Options.Value.GetUrlExpirationMinutes : 60;
        var expiresUtc = DateTime.UtcNow.AddMinutes(minutes);
        var url = s3.CreatePresignedGet(media.StoragePath, expiresUtc);
        return new MediaPlaybackUrlResponse(url, new DateTimeOffset(expiresUtc, TimeSpan.Zero));
    }
}
