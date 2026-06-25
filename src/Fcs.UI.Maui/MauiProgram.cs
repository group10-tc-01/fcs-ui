using Fcs.UI.Application;
using Fcs.UI.Application.Interfaces;
using Fcs.UI.Application.Services;
using Fcs.UI.Cache;
using Fcs.UI.Infrastructure;
using Fcs.UI.Maui.Infrastructure;
using Maui.Biometric;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using System.Reflection;
#if ANDROID
using Fcs.UI.Maui.Infrastructure.Background;
#endif

namespace Fcs.UI.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBiometricAuthentication()
            .ConfigureEssentials(essentials =>
            {
                essentials
                    .AddAppAction("donate", "Doação rápida", subtitle: "Fazer uma doação")
                    .AddAppAction("history", "Últimas doações", subtitle: "Ver histórico")
                    .OnAppAction(App.HandleAppActions);
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        var assembly = Assembly.GetExecutingAssembly();

        var configBuilder = new ConfigurationBuilder();

        using var stream = assembly.GetManifestResourceStream("Fcs.UI.Maui.appsettings.json");
        configBuilder.AddJsonStream(stream!);

#if ANDROID
        using var androidStream = assembly.GetManifestResourceStream("Fcs.UI.Maui.appsettings.android.json");
        if (androidStream != null)
        {
            configBuilder.AddJsonStream(androidStream);
        }
#endif

        var config = configBuilder.Build();

        builder.Services.AddSingleton<IConfiguration>(config);

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(config);

        builder.Services.AddSingleton<ITokenStorage, SecureTokenStorage>();
        builder.Services.AddSingleton<IConnectivityService, MauiConnectivityService>();
        builder.Services.AddSingleton<IBiometricService, BiometricAuthService>();
        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddTransient<DeepLinkNavigationService>();
        builder.Services.AddSingleton(_ =>
            new SqliteCacheContext(Path.Combine(FileSystem.AppDataDirectory, "cache.db")));

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
