namespace Application.Common.Settings.Identity
{
    public class LockoutSettings
    {
        public bool AllowedForNewUsers { get; init; }
        public int MaxFailedAccessAttempts { get; init; }
        public int DefaultLockoutTimeSpan { get; init; }
    }
}