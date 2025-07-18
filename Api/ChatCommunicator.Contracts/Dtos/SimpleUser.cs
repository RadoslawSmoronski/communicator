namespace ChatCommunicator.Contracts.Dtos
{
    public class SimpleUserDto
    {
        public required Guid Id { get; set; }
        public required string userName { get; set; }
    }
}
