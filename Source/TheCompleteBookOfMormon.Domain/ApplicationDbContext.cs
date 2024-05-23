using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TheCompleteBookOfMormon.Domain.Features.Editions;
using TheCompleteBookOfMormon.Domain.Features.Pages;

namespace TheCompleteBookOfMormon.Domain;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    public DbSet<Edition> Editions { get; init; } = null!;
    public DbSet<Page> Pages { get; init; } = null!;

    internal void EnableChangeTracking()
    {
        ChangeTracker.AutoDetectChangesEnabled = true;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        foreach (var entry in ChangeTracker.Entries().ToList())
            if (entry.Entity is not null)
                entry.State = EntityState.Detached;

        return result;
    }
}
