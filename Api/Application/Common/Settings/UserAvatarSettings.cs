namespace Application.Settings
{
    public class UserAvatarSettings
    {
        public required int MaxFileSizeBytes { get; init; }
        public required int MaxImageWidth { get; init; }
        public required int MaxImageHeight { get; init; }
        public required int MinImageWidth { get; init; }
        public required int MinImageHeight { get; init; }
        public required string[] AllowedExtensions { get; init; }
    }
}