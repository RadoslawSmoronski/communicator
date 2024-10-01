using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Emit;
using Api.Models.Friendship;

namespace Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserAccount>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<FriendshipInvitation> FriendshipInvitations { get; set; }

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
        }

    }
}