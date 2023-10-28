using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocr.Config;

namespace Ocr;

internal static class RegisterOptionsExtension
{
    public static HostApplicationBuilder RegisterOptions<T>(
        this HostApplicationBuilder builder)
    where T : class, IConfigSettings
    {
        builder
            .Services
            .AddOptions<T>()
            .BindConfiguration(T.GetSectionName())
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return builder;
    }
}
