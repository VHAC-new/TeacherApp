using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Api.Application.Admin.Exercises;
using TeacherApp.Api.Common;
using TeacherApp.Contracts.Exercises;

namespace TeacherApp.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/exercises")]
[Authorize(Roles = Roles.Admin)]
public sealed class ExercisesAdminController(IAdminExerciseService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExerciseResponse>>> List([FromQuery] Guid? lessonId, CancellationToken cancellationToken)
        => Ok(await service.ListAsync(lessonId, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExerciseResponse>> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var exercise = await service.GetAsync(id, cancellationToken);
        return exercise is null ? NotFound() : Ok(exercise);
    }

    [HttpPost]
    public async Task<ActionResult<ExerciseResponse>> Create([FromBody] CreateExerciseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExerciseResponse>> Update([FromRoute] Guid id, [FromBody] UpdateExerciseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await service.UpdateAsync(id, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
