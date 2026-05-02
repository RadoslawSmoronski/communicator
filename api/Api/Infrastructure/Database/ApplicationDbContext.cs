using Domain.Entities;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<UserAccount, ApplicationRole, Guid>(options)
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<FriendshipEntity> Friendships { get; set; }
        public DbSet<FriendshipInvitationEntity> FriendshipInvitations { get; set; }
        public DbSet<ConversationEntity> Conversations { get; set; }
        public DbSet<MessageEntity> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Conversations -> Users (unchanged)
            builder.Entity<ConversationEntity>().HasKey(c => c.Id);
            builder.Entity<ConversationEntity>()
                .HasOne(c => c.User1).WithMany().HasForeignKey(c => c.User1Id).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ConversationEntity>()
                .HasOne(c => c.User2).WithMany().HasForeignKey(c => c.User2Id).OnDelete(DeleteBehavior.Restrict);

            // One-to-one: Conversation.LastMessage -> Message
            builder.Entity<ConversationEntity>()
                .HasOne(c => c.LastMessage)
                .WithOne()
                .HasForeignKey<ConversationEntity>(c => c.LastMessageId)
                .OnDelete(DeleteBehavior.SetNull);

            // One-to-many: Conversation -> Messages using Message.ConversationId
            builder.Entity<MessageEntity>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Sender relationship
            builder.Entity<MessageEntity>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Helpful index
            builder.Entity<MessageEntity>().HasIndex(m => new { m.ConversationId, m.CreatedAt });

            // Friendships/Invitations config stays as you had it
        }
    }
}