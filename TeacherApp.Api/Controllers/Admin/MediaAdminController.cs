using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Api.Application.Admin.Media;
using TeacherApp.Api.Common;
using TeacherApp.Api.Infrastructure.Storage;
using TeacherApp.Contracts.Media;

namespace TeacherApp.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/media")]
[Authorize(Roles = Roles.Admin)]
public sealed class MediaAdminController(IAdminMediaService mediaService, IS3MediaOperations s3) : ControllerBase
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "audio/mpeg", "audio/wav", "audio/ogg", "audio/mp4",
        "image/jpeg", "image/png", "image/gif", "image/webp",
    ];

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MediaResponse>>> List(CancellationToken cancellationToken)
        => Ok(await mediaService.ListAsync(cancellationToken));

    [HttpPost("upload-url")]
    public async Task<ActionResult<InitMediaUploadResponse>> InitUpload(
        [FromBody] InitMediaUploadRequest request, CancellationToken cancellationToken)
    {
        if (!s3.IsEnabled)
        {
            return BadRequest(new { error = "S3 is not configured. Use multipart POST /api/v1/admin/media for local disk upload." });
        }

        try
        {
            var result = await mediaService.InitPresignedUploadAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<MediaResponse>> CompleteUpload(
        [FromRoute] Guid id, CancellationToken cancellationToken)
    {
        if (!s3.IsEnabled)
        {
            return BadRequest(new { error = "S3 is not configured." });
        }

        try
        {
            var result = await mediaService.CompletePresignedUploadAsync(id, cancellationToken);
            if (result is null)
            {
                return NotFound(new { error = "Media not found, or object not present in S3 yet (retry after PUT completes)." });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Failed to verify S3"))
        {
            return StatusCode(502, new { error = "Could not verify the uploaded object in S3. Check IAM permissions and bucket configuration.", detail = ex.Message });
        }
    }

    [HttpPost]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<ActionResult<MediaResponse>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (s3.IsEnabled)
        {
            return BadRequest(new { error = "S3 is enabled: use POST /api/v1/admin/media/upload-url, PUT file to the returned URL, then POST /api/v1/admin/media/{id}/complete." });
        }

        if (file.Length == 0)
        {
            return BadRequest(new { error = "File is empty." });
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return BadRequest(new { error = $"Content type '{file.ContentType}' is not allowed." });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await mediaService.UploadAsync(file.FileName, file.ContentType, stream, cancellationToken);
            return Created($"/api/v1/media/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediaService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("cleanup-incomplete")]
    public async Task<IActionResult> CleanupIncomplete(
        [FromQuery] int olderThanHours = 24, CancellationToken cancellationToken = default)
    {
        if (olderThanHours < 1) olderThanHours = 1;
        var removed = await mediaService.CleanupIncompleteAsync(
            TimeSpan.FromHours(olderThanHours), cancellationToken);
        return Ok(new { removed });
    }
}
