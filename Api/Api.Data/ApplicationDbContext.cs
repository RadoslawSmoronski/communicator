using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Api.Models;
using Microsoft.AspNetCore.Identity;

namespace Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserAccount>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);
        }

    }
}