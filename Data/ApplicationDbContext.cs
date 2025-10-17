using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AnimalCrossingTracker.Models;

namespace AnimalCrossingTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Collectible> Collectibles { get; set; }
        public DbSet<UserCollectible> UserCollectibles { get; set; }
    }
}
