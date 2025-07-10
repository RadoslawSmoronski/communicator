using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChatCommunicator.Contracts;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Emit;
using ChatCommunicator.Contracts.Friendship;
using ChatCommunicator.Contracts.Chat;

namespace ChatCommunicator.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<UserAccount, ApplicationRole, Guid>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<FriendshipInvitation> FriendshipInvitations { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);

            builder.Entity<Friendship>()
            .HasKey(f => f.Id);

            builder.Entity<Friendship>()
                .HasOne(f => f.User1)
                .WithMany()
                .HasForeignKey(f => f.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Friendship>()
                 .HasOne(f => f.User2)
                 .WithMany()
                 .HasForeignKey(f => f.User2Id)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Friendship>()
                .HasIndex(f => new { f.User1Id, f.User2Id })
                .IsUnique();

            builder.Entity<FriendshipInvitation>()
                .HasKey(f => f.Id);

            builder.Entity<FriendshipInvitation>()
                .HasOne(f => f.SenderUser)
                .WithMany()
                .HasForeignKey(f => f.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FriendshipInvitation>()
                 .HasOne(f => f.RecipientUser)
                 .WithMany()
                 .HasForeignKey(f => f.RecipientId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FriendshipInvitation>()
                .HasIndex(f => new { f.SenderId, f.RecipientId })
                .IsUnique();

            builder.Entity<Conversation>()
                 .HasKey(c => c.Id);

            builder.Entity<Conversation>()
                .HasOne(c => c.User1)
                .WithMany()
                .HasForeignKey(c => c.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Conversation>()
                .HasOne(c => c.User2)
                .WithMany()
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Conversation>()
                .HasOne(c => c.LastMessage)
                .WithMany()
                .HasForeignKey(c => c.LastMessageId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Conversation>()
                .HasIndex(c => new { c.User1Id, c.User2Id }).IsUnique();

            builder.Entity<Message>()
                .HasKey(m => m.Id);

            builder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasIndex(m => new { m.ConversationId, m.Timestamp });

            builder.Entity<Message>()
                .Property(m => m.IsRead)
                .HasDefaultValue(false);
        }

    }
}