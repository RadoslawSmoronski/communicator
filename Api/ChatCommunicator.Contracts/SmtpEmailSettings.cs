namespace ChatCommunicator.API.Models
{
    public class SmtpEmailSettings
    {
        public required string SmtpHost { get; set; }
        public required int SmtpPort { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string FromAddress { get; set; }
    }
}
