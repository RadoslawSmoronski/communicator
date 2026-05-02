using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Entities
{
    [Table("Conversations")]
    public class ConversationEntity : BaseEntityInfrastructure
    {
        public required Guid User1Id { get; set; }
        public required Guid User2Id { get; set; }
        [ForeignKey(nameof(User1Id))]
        public required UserAccount User1 { get; set; }
        [ForeignKey(nameof(User2Id))]
        public required UserAccount User2 { get; set; }

        public Guid? LastMessageId { get; set; }
        [ForeignKey(nameof(LastMessageId))]
        public MessageEntity? LastMessage { get; set; }
        public ICollection<MessageEntity> Messages { get; set; } = [];

        public Guid? User1LastReadMessageId { get; set; }
        public Guid? User2LastReadMessageId { get; set; }
        public DateTime? LastMessageTime { get; set; }
    }
}
