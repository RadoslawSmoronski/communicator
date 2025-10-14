namespace Application.DTOs
{
    public class UserToInviteDto
    {
        public required Guid Id { get; set; }
        public required string UserName { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsInvited { get; set; } = false;
    }
}
