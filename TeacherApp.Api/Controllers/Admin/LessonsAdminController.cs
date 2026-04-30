using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Api.Application.Admin.Lessons;
using TeacherApp.Api.Common;
using TeacherApp.Contracts.Lessons;

namespace TeacherApp.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/lessons")]
[Authorize(Roles = Roles.Admin)]
public sealed class LessonsAdminController(IAdminLessonService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LessonResponse>>> List([FromQuery] Guid? moduleId, CancellationToken cancellationToken)
        => Ok(await service.ListAsync(moduleId, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LessonResponse>> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var lesson = await service.GetAsync(id, cancellationToken);
        return lesson is null ? NotFound() : Ok(lesson);
    }

    [HttpPost]
    public async Task<ActionResult<LessonResponse>> Create([FromBody] CreateLessonRequest request, CancellationToken cancellationToken)
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
    public async Task<ActionResult<LessonResponse>> Update([FromRoute] Guid id, [FromBody] UpdateLessonRequest request, CancellationToken cancellationToken)
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
