using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Api.Application.Catalog;
using TeacherApp.Contracts.Lessons;
using TeacherApp.Contracts.Modules;

namespace TeacherApp.Api.Controllers;

[ApiController]
[Route("api/v1/modules")]
[Authorize]
public sealed class ModulesController(ICatalogService catalog) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ModuleResponse>>> List(CancellationToken cancellationToken)
        => Ok(await catalog.ListModulesAsync(cancellationToken));

    [HttpGet("{id:guid}/lessons")]
    public async Task<ActionResult<IReadOnlyList<LessonResponse>>> Lessons([FromRoute] Guid id, CancellationToken cancellationToken)
        => Ok(await catalog.ListLessonsByModuleAsync(id, cancellationToken));
}
