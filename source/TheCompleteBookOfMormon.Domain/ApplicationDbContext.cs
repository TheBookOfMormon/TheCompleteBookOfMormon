using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TheCompleteBookOfMormon.Domain.Editions;

namespace TheCompleteBookOfMormon.Domain;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Edition> Editions { get; init; }
}
