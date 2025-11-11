namespace Application.Common.Settings
{
    public class RefreshTokenSettings
    {
        public required int RefreshTokenLifeInSeconds { get; set; }
        public required int RefreshTokenCleanUpIntercalInSeconds { get; set; }
    }
}
