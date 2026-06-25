using Microsoft.AspNetCore.Components;

namespace Fcs.UI.Maui.Infrastructure;

public sealed class DeepLinkNavigationService
{
    private readonly NavigationManager _nav;

    public DeepLinkNavigationService(NavigationManager nav)
    {
        _nav = nav;
    }

    public void TryNavigateFromDeepLink()
    {
        var uri = DeepLinkHandler.ConsumePending();
        if (uri is null) return;

        var path = uri switch
        {
            "fcs://donate" => "/donate",
            "fcs://history" => "/history",
            _ => null
        };

        if (path is not null)
            _nav.NavigateTo(path);
    }
}