using Microsoft.Extensions.DependencyInjection;
using TheCompleteBookOfMormon.Domain.Editions;

namespace TheCompleteBookOfMormon.Domain;

public static class Services
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<IEditionsRepository, EditionsRepository>();
    }
}
