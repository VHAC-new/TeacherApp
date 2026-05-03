using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Api.Application.Exercises;
using TeacherApp.Contracts.FinalExercises;

namespace TeacherApp.Api.Controllers;

[ApiController]
[Route("api/v1/final-exercises")]
[Authorize]
public sealed class FinalExercisesController(IExerciseSubmitService submitService) : ControllerBase
{
    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<SubmitFinalExerciseResponse>> Submit(
        [FromRoute] Guid id, [FromBody] SubmitFinalExerciseRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Answer))
            return BadRequest(new { error = "Answer is required." });

        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var result = await submitService.SubmitFinalAsync(id, userId, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(sub) && Guid.TryParse(sub, out userId);
    }
}
