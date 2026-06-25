using Fcs.UI.Maui.Infrastructure;

namespace Fcs.UI.Maui;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "Fcs.UI.Maui" };
    }

    public static void HandleAppActions(AppAction appAction)
    {
        Current?.Dispatcher.Dispatch(() =>
        {
            var uri = appAction.Id switch
            {
                "donate" => "fcs://donate",
                "history" => "fcs://history",
                _ => null
            };
            if (uri is not null)
                DeepLinkHandler.Handle(uri);
        });
    }
}
