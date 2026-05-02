namespace Application.Common.Settings.Identity
{
    public record class PasswordSettings
    {
        public bool RequireDigit { get; init; }
        public int RequiredLength { get; init; }
        public bool RequireLowercase { get; init; }
        public bool RequireUppercase { get; init; }
        public bool RequireNonAlphanumeric { get; init; }
    }
}
