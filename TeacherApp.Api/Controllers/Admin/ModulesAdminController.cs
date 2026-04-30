using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Api.Application.Admin.Modules;
using TeacherApp.Api.Common;
using TeacherApp.Contracts.Modules;

namespace TeacherApp.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/modules")]
[Authorize(Roles = Roles.Admin)]
public sealed class ModulesAdminController(IAdminModuleService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ModuleResponse>>> List(CancellationToken cancellationToken)
        => Ok(await service.ListAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ModuleResponse>> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var module = await service.GetAsync(id, cancellationToken);
        return module is null ? NotFound() : Ok(module);
    }

    [HttpPost]
    public async Task<ActionResult<ModuleResponse>> Create([FromBody] CreateModuleRequest request, CancellationToken cancellationToken)
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
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ModuleResponse>> Update([FromRoute] Guid id, [FromBody] UpdateModuleRequest request, CancellationToken cancellationToken)
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
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

