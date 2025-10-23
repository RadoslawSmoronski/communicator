using Infrastructure.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Database
{
    [Table("Messages")]
    public class MessageEntity : BaseEntityInfrastructure, IInfraEntity
    {
        public required Guid ConversationId { get; set; }
        public required Guid SenderId { get; set; }
        public required UserAccount Sender { get; set; }
        public required string Content { get; set; }
        public required DateTime Timestamp { get; set; }
    }
}
