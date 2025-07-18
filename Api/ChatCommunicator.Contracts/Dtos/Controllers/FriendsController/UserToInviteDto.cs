namespace ChatCommunicator.Contracts.Dtos.Controllers.FriendsController
{
    public class UserToInviteDto
    {
        public required Guid Id { get; set; }
        public required string UserName { get; set; } 
        public bool IsInvited { get; set; } = false;
    }
}
