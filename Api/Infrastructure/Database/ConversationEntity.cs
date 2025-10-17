using Domain.Entities;
using Infrastructure.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Database
{
    [Table("Conversations")]
    public class ConversationEntity : BaseEntityInfrastructure, IInfraEntity
    {
        public required Guid User1Id { get; set; }
        public required Guid User2Id { get; set; }

        public required UserAccount User1 { get; set; }
        public required UserAccount User2 { get; set; }

        public Guid? LastMessageId { get; set; }
        public MessageEntity? LastMessage { get; set; }
        public ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();

        public Guid? User1LastReadMessageId { get; set; }
        public Guid? User2LastReadMessageId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMessageTime { get; set; }
    }
}
