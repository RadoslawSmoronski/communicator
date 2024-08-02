using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Api.Models;
using Microsoft.AspNetCore.Identity;

namespace Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserAccount, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

    }
}