using TeacherApp.Contracts.Media;

namespace TeacherApp.Api.Application.Admin.Media;

public interface IAdminMediaService
{
    Task<IReadOnlyList<MediaResponse>> ListAsync(CancellationToken cancellationToken);
    Task<MediaResponse> UploadAsync(string fileName, string contentType, Stream content, CancellationToken cancellationToken);
    Task<InitMediaUploadResponse> InitPresignedUploadAsync(InitMediaUploadRequest request, CancellationToken cancellationToken);
    Task<MediaResponse?> CompletePresignedUploadAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<int> CleanupIncompleteAsync(TimeSpan olderThan, CancellationToken cancellationToken);
}
