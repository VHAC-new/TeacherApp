using System.Net;
using System.Net.Http.Json;
using TeacherApp.Contracts.Media;
using TeacherApp.Tests.Fixtures;

namespace TeacherApp.Tests;

public sealed class MediaAdminIntegrationTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public MediaAdminIntegrationTests(TestWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Upload_url_returns_bad_request_when_s3_disabled()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid(), "admin@test.com", "Admin");

        var response = await client.PostAsJsonAsync("/api/v1/admin/media/upload-url",
            new InitMediaUploadRequest("test.mp3", "audio/mpeg"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Multipart_upload_works_when_s3_disabled()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid(), "admin@test.com", "Admin");

        using var content = new MultipartFormDataContent();
        var fileBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");

        var response = await client.PostAsync("/api/v1/admin/media", content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<MediaResponse>();
        Assert.NotNull(result);
        Assert.Equal("test.jpg", result.FileName);
        Assert.True(result.UploadCompleted);

        var listResponse = await client.GetFromJsonAsync<List<MediaResponse>>("/api/v1/admin/media");
        Assert.NotNull(listResponse);
        Assert.Single(listResponse);

        var deleteResponse = await client.DeleteAsync($"/api/v1/admin/media/{result.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Non_admin_cannot_list_media()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid(), "student@test.com", "Student");
        var response = await client.GetAsync("/api/v1/admin/media");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
