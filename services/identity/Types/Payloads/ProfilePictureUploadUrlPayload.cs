public class ProfilePictureUploadUrlPayload
{
    public bool Successful { get; set; } = false;
    public string? Url { get; set; } = default!;
    public string? Error { get; set; } = default!;
}
