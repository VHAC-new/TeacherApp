using TeacherApp.Contracts.Media;

namespace TeacherApp.Tests;

public sealed class MediaContractsTests
{
    [Fact]
    public void InitMediaUploadResponse_carries_presign_fields()
    {
        var expires = DateTimeOffset.Parse("2026-05-03T12:00:00Z");
        var r = new InitMediaUploadResponse(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "https://bucket.s3.amazonaws.com/key?X-Amz-Signature=abc",
            "media/00000000000000000000000000000001.mp3",
            expires);

        Assert.Contains("amazonaws.com", r.UploadUrl);
        Assert.Equal("media/00000000000000000000000000000001.mp3", r.ObjectKey);
        Assert.Equal(expires, r.ExpiresAtUtc);
    }
}
