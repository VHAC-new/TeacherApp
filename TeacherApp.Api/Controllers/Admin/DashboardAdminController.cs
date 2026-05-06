using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Api.Application.Admin.Dashboard;
using TeacherApp.Api.Common;
using TeacherApp.Contracts.Admin;

namespace TeacherApp.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/dashboard")]
[Authorize(Roles = Roles.Admin)]
public sealed class DashboardAdminController(IDashboardAdminService service) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardAdminStatsResponse>> Stats(CancellationToken cancellationToken)
        => Ok(await service.GetStatsAsync(cancellationToken));

    [HttpGet("completed-lessons")]
    public async Task<ActionResult<IReadOnlyList<CompletedLessonAdminRowResponse>>> CompletedLessons(
        [FromQuery] Guid? studentId,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => Ok(await service.ListCompletedLessonsAsync(studentId, take, cancellationToken));
}
