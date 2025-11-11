using Infrastructure.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Database
{
    [Table("Messages")]
    public class MessageEntity : BaseEntityInfrastructure
    {
        public required Guid ConversationId { get; set; }
        [ForeignKey(nameof(ConversationId))]
        public ConversationEntity? Conversation { get; set; }
        public required Guid SenderId { get; set; }
        [ForeignKey(nameof(SenderId))]
        public UserAccount? Sender { get; set; }
        public required string Content { get; set; }
    }
}
