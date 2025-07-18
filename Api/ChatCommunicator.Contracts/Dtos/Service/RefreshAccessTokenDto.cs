namespace ChatCommunicator.Contracts.Dtos.Service
{
    public class RefreshAccessTokenDto
    {
        public required string AccessToken { get; set; }
        public required Guid RefreshToken { get; set; }
    }
}
