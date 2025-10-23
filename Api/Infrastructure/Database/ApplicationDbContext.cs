using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database
{
    public class ApplicationDbContext : IdentityDbContext<UserAccount, ApplicationRole, Guid>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<FriendshipEntity> Friendships { get; set; }
        public DbSet<FriendshipInvitationEntity> FriendshipInvitations { get; set; }
        public DbSet<ConversationEntity> Conversations { get; set; }
        public DbSet<MessageEntity> Messages { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>()
                .HasOne<UserAccount>()
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FriendshipEntity>().HasKey(f => f.Id);
            builder.Entity<FriendshipEntity>()
                .HasOne(f => f.User1).WithMany().HasForeignKey(f => f.User1Id).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<FriendshipEntity>()
                .HasOne(f => f.User2).WithMany().HasForeignKey(f => f.User2Id).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<FriendshipEntity>()
                .HasIndex(f => new { f.User1Id, f.User2Id }).IsUnique();

            builder.Entity<FriendshipInvitationEntity>()
                .HasOne(x => x.SenderUser).WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<FriendshipInvitationEntity>()
                .HasOne(x => x.RecipientUser).WithMany().HasForeignKey(x => x.RecipientId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ConversationEntity>().HasKey(c => c.Id);
            builder.Entity<ConversationEntity>()
                .HasOne(c => c.User1).WithMany().HasForeignKey(c => c.User1Id).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ConversationEntity>()
                .HasOne(c => c.User2).WithMany().HasForeignKey(c => c.User2Id).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ConversationEntity>()
                .HasOne(c => c.LastMessage)
                .WithOne()
                .HasForeignKey<ConversationEntity>(c => c.LastMessageId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<MessageEntity>()
                  .HasOne(m => m.Sender)
                  .WithMany()
                  .HasForeignKey(m => m.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}