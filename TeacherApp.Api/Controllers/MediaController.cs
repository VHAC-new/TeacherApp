using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherApp.Api.Application.Media;
using TeacherApp.Api.Data;
using TeacherApp.Api.Infrastructure.Storage;
using TeacherApp.Contracts.Media;

namespace TeacherApp.Api.Controllers;

[ApiController]
[Route("api/v1/media")]
[Authorize]
public sealed class MediaController(
    AppDbContext db,
    IConfiguration configuration,
    IMediaPlaybackService playbackService,
    IS3MediaOperations s3) : ControllerBase
{
    [HttpGet("{id:guid}/playback-url")]
    public async Task<ActionResult<MediaPlaybackUrlResponse>> PlaybackUrl(
        [FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await playbackService.GetPresignedPlaybackUrlAsync(id, cancellationToken);
        if (result is not null)
        {
            return Ok(result);
        }

        if (!s3.IsEnabled)
        {
            return BadRequest(new { error = "S3 is not configured. Use GET /api/v1/media/{id} to stream from local storage." });
        }

        return NotFound();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var media = await db.MediaFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (media is null)
        {
            return NotFound();
        }

        if (s3.IsEnabled)
        {
            if (!media.UploadCompleted)
            {
                return NotFound();
            }

            return NotFound(new
            {
                error = "Media is stored in S3. Use GET /api/v1/media/{id}/playback-url for a temporary playback URL.",
            });
        }

        if (!media.UploadCompleted)
        {
            return NotFound();
        }

        var mediaRoot = configuration["Media:StoragePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "media-files");

        var fullPath = Path.Combine(mediaRoot, media.StoragePath);
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        var stream = System.IO.File.OpenRead(fullPath);
        return File(stream, media.ContentType, media.FileName);
    }
}
