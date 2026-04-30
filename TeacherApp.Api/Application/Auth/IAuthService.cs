using TeacherApp.Contracts.Auth;

namespace TeacherApp.Api.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}

