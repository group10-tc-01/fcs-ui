namespace Fcs.UI.Maui;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

#if ANDROID
    protected override void OnAppearing()
    {
        base.OnAppearing();

        var resources = Android.App.Application.Context.Resources;
        var resourceId = resources!.GetIdentifier("status_bar_height", "dimen", "android");
        if (resourceId > 0)
        {
            var statusBarHeight = resources.GetDimensionPixelSize(resourceId);
            var density = DeviceDisplay.MainDisplayInfo.Density;
            Padding = new Thickness(0, statusBarHeight / density, 0, 0);
        }
    }
#endif
}
