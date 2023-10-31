using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheCompleteBookOfMormon.Domain.Editions;

namespace TheCompleteBookOfMormon.Domain;

public static class Services
{
    public static void Register(IServiceCollection services, string dbConnectionString)
    {
        services.AddScoped<IEditionsRepository, EditionsRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(dbConnectionString));
    }
}
