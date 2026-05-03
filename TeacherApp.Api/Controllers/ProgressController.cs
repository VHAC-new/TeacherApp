using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Api.Application.Progress;
using TeacherApp.Contracts.Progress;

namespace TeacherApp.Api.Controllers;

[ApiController]
[Route("api/v1/progress")]
[Authorize]
public sealed class ProgressController(IProgressService progressService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<OverallProgressResponse>> GetOverall(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        return Ok(await progressService.GetOverallAsync(userId, cancellationToken));
    }

    [HttpGet("modules/{moduleId:guid}")]
    public async Task<ActionResult<ModuleProgressResponse>> GetModule(
        [FromRoute] Guid moduleId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var result = await progressService.GetModuleAsync(userId, moduleId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(sub) && Guid.TryParse(sub, out userId);
    }
}
