using TeacherApp.Contracts.Lessons;

namespace TeacherApp.Api.Application.Admin.Lessons;

public interface IAdminLessonService
{
    Task<IReadOnlyList<LessonResponse>> ListAsync(Guid? moduleId, CancellationToken cancellationToken);
    Task<LessonResponse?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<LessonResponse> CreateAsync(CreateLessonRequest request, CancellationToken cancellationToken);
    Task<LessonResponse?> UpdateAsync(Guid id, UpdateLessonRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
