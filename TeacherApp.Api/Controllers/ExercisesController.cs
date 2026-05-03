using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Api.Application.Exercises;
using TeacherApp.Contracts.Exercises;

namespace TeacherApp.Api.Controllers;

[ApiController]
[Route("api/v1/exercises")]
[Authorize]
public sealed class ExercisesController(IExerciseSubmitService submitService) : ControllerBase
{
    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<SubmitExerciseResponse>> Submit(
        [FromRoute] Guid id, [FromBody] SubmitExerciseRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Answer))
            return BadRequest(new { error = "Answer is required." });

        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var result = await submitService.SubmitAsync(id, userId, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(sub) && Guid.TryParse(sub, out userId);
    }
}
