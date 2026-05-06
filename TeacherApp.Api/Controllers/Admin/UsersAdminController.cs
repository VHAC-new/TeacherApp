using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Api.Application.Admin.Dashboard;
using TeacherApp.Api.Common;
using TeacherApp.Contracts.Admin;

namespace TeacherApp.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = Roles.Admin)]
public sealed class UsersAdminController(IDashboardAdminService service) : ControllerBase
{
    [HttpGet("students")]
    public async Task<ActionResult<IReadOnlyList<AdminStudentResponse>>> Students(CancellationToken cancellationToken)
        => Ok(await service.ListStudentsAsync(cancellationToken));
}
