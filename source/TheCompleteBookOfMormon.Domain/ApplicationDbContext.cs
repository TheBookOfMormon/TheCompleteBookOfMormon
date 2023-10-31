using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TheCompleteBookOfMormon.Domain.Editions;

namespace TheCompleteBookOfMormon.Domain;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    public DbSet<Edition> Editions { get; init; }

    internal void EnableChangeTracking()
    {
        ChangeTracker.AutoDetectChangesEnabled = true;
    }
}
