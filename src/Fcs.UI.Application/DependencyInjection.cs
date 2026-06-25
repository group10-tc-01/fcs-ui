using Fcs.UI.Application.ViewModels;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Fcs.UI.Application;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTransient<DonationViewModel>();
        services.AddTransient<CampaignListViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<AdminCampaignListViewModel>();
        services.AddTransient<AdminCampaignEditViewModel>();
        services.AddTransient<CampaignSelectionViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<DonationHistoryViewModel>();
        services.AddTransient<AdminDonationHistoryViewModel>();
        services.AddTransient<ProfileViewModel>();

        return services;
    }
}
