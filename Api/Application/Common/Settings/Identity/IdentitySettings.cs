namespace Application.Common.Settings.Identity
{
    public class IdentitySettings
    {
        public PasswordSettings PasswordSettings { get; init; } = new();
        public LockoutSettings LockoutSettings { get; init; } = new();
        public UserSettings UserSettings { get; init; } = new();
        public SignInSettings SignInSettings { get; init; } = new();
    }
}