using TeacherApp.Contracts.Media;

namespace TeacherApp.Api.Application.Media;

public interface IMediaPlaybackService
{
    Task<MediaPlaybackUrlResponse?> GetPresignedPlaybackUrlAsync(Guid mediaId, CancellationToken cancellationToken);
}
