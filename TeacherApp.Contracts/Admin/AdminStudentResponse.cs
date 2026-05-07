namespace TeacherApp.Contracts.Admin;

public sealed record AdminStudentResponse(
    Guid Id,
    string Email,
    bool IsActive);
