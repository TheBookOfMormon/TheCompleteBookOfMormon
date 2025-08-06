using DocumentsModel;
using System.Text.Json;
using WordsAnalysis.Services;

namespace WordsAnalysis;

static class Bootstrapper
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IConfirmService, ConfirmService>();
        services.AddSingleton<IDictionaryService, DictionaryService>();
        services.AddScoped<IHtmlService, HtmlService>();
        services.AddSingleton<IImageRepository, ImageRepository>();
        services.Configure<JsonSerializerOptions>(x =>
        {
            x.TypeInfoResolver = ModelJsonContext.Default;
        });
    }
}
