namespace ChatCommunicator.Contracts
{
    public class ConfirmEmailMessageSettings
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required string Address { get; set; }
    }
}
