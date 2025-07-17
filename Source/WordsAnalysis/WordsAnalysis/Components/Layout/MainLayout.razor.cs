using Microsoft.AspNetCore.Components.Routing;

namespace WordsAnalysis.Components.Layout;
public partial class MainLayout
{
    protected override void OnInitialized()
    {
        base.OnInitialized();
        NavigationManager.RegisterLocationChangingHandler(PreventNavigation);
    }

    private ValueTask PreventNavigation(LocationChangingContext context)
    {
        if (!context.IsNavigationIntercepted)
            context.PreventNavigation();
        return ValueTask.CompletedTask;
    }
}