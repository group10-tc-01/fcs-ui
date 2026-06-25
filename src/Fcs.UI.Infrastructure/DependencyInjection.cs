using Fcs.UI.Application.Interfaces;
using Fcs.UI.Cache;
using Fcs.UI.Infrastructure.Http;
using Fcs.UI.Infrastructure.Services;
using Fcs.UI.Infrastructure.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Fcs.UI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICacheService, SqliteCacheService>();
        services.AddScoped<IEventStore, SqliteEventStore>();
        services.AddScoped<SyncService>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IDonationRepository, DonationRepository>();

        services.AddTransient<AuthTokenHandler>();

        services.AddRefitClient<IAuthApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new(config["Api:Identity"]!));
        services.AddRefitClient<ICampaignApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new(config["Api:Campaign"]!);
                c.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddHttpMessageHandler<AuthTokenHandler>();
        services.AddRefitClient<IDonationApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new(config["Api:Donations"]!);
                c.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddHttpMessageHandler<AuthTokenHandler>();

        TelemetrySetup.ConfigureOpenTelemetry();
        TelemetrySetup.ConfigureSerilog();

        return services;
    }
}
