using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheCompleteBookOfMormon.Domain.Features.Editions;
using TheCompleteBookOfMormon.Domain.Features.Pages;

namespace TheCompleteBookOfMormon.Domain;

public static class Services
{
    public static void Register(IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IEditionsRepository, EditionsRepository>();
        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        string dbConnectionString = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(x =>
        {
            x.UseSqlServer(dbConnectionString);
            x.UseLazyLoadingProxies();
            x.UseLoggerFactory(LoggerFactory.Create(builder =>
            {
                builder.AddDebug().SetMinimumLevel(LogLevel.Debug);
            }));
        });
    }
}
