using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Api.Application.Catalog;
using TeacherApp.Contracts.Exercises;
using TeacherApp.Contracts.FinalExercises;
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

    [HttpGet("{moduleId:guid}/lessons/{lessonId:guid}/exercises")]
    public async Task<ActionResult<IReadOnlyList<ExerciseStudentResponse>>> Exercises(
        [FromRoute] Guid lessonId, CancellationToken cancellationToken)
        => Ok(await catalog.ListExercisesByLessonAsync(lessonId, cancellationToken));

    [HttpGet("{moduleId:guid}/final-exercises")]
    public async Task<ActionResult<IReadOnlyList<FinalExerciseStudentResponse>>> FinalExercises(
        [FromRoute] Guid moduleId, CancellationToken cancellationToken)
        => Ok(await catalog.ListFinalExercisesByModuleAsync(moduleId, cancellationToken));
}
