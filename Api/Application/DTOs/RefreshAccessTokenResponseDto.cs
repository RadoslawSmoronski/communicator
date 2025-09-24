namespace Application.DTOs
{
    public class RefreshAccessTokenResponseDto
    {
        public required string AccessToken { get; set; }
        public required Guid RefreshToken { get; set; }
    }
}
