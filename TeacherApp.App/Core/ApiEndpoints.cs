namespace TeacherApp.App.Core;

internal static class ApiEndpoints
{
    /// <summary>VPS — API pública via nginx (porta 443). O IP direto não roteia /api.</summary>
    internal const string VpsApi = "https://api.dhschool.com.br";

    /// <summary>URL usada em builds Release (APK).</summary>
    internal const string VpsRelease = VpsApi;
}
