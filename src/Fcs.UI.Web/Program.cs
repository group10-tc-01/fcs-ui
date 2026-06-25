using Fcs.UI.Application;
using Fcs.UI.Application.Interfaces;
using Fcs.UI.Application.Services;
using Fcs.UI.Application.ViewModels;
using Fcs.UI.Cache;
using Fcs.UI.Infrastructure.Http;
using Fcs.UI.Infrastructure.Services;
using Fcs.UI.Web.Infrastructure;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Fcs.UI.Web.Components.App>("#app");

builder.Services.AddMudServices();
builder.Services.AddApplication();

builder.Services.AddScoped<ITokenStorage, WebTokenStorage>();
builder.Services.AddScoped<IConnectivityService, WebConnectivityService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDonationRepository, DonationRepository>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<LoginViewModel>();
builder.Services.AddScoped<DonationHistoryViewModel>();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<IEventStore, SqliteEventStore>();
builder.Services.AddSingleton(_ => new SqliteCacheContext(":memory:"));

builder.Services.AddTransient<AuthTokenHandler>();

builder.Services.AddRefitClient<IAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new("http://localhost:5001"));
builder.Services.AddRefitClient<ICampaignApi>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new("http://localhost:5002");
        c.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddRefitClient<IDonationApi>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new("http://localhost:5003");
        c.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddHttpMessageHandler<AuthTokenHandler>();

var host = builder.Build();
await host.RunAsync();
