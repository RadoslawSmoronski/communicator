namespace ChatCommunicator.Contracts.Dtos.Service
{
    public class RefreshAccessTokenResponseDto
    {
        public bool Succeeded { get; set; } = false;
        public required string Message { get; set; }
        public required string AccessToken { get; set; }
    }
}
