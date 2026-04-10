using DocumentsModel;
using System.Text.Json;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.AppLayer.Services;
using WordsAnalysis.Services;

namespace WordsAnalysis;

static class Bootstrapper
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAppPreferences, AppPreferences>();
        services.AddScoped<IConfirmService, ConfirmService>();
        services.AddSingleton<IDataPaths, DataPaths>();
        services.AddSingleton<IDictionaryService, DictionaryService>();
        services.AddScoped<IHtmlService, HtmlService>();
        services.AddSingleton<IImageRepository, ImageRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISyncDocumentsDialogService, SyncDocumentsDialogService>();
        services.Configure<JsonSerializerOptions>(x =>
        {
            x.TypeInfoResolver = ModelJsonContext.Default;
        });
    }
}
