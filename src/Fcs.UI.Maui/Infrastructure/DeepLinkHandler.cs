namespace Fcs.UI.Maui.Infrastructure;

public static class DeepLinkHandler
{
    private static string? _pendingUri;

    public static void Handle(string uri)
    {
        _pendingUri = uri;
    }

    public static string? ConsumePending()
    {
        var uri = _pendingUri;
        _pendingUri = null;
        return uri;
    }
}