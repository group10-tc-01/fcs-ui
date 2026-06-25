using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;

namespace Fcs.UI.Application.ViewModels;

public sealed partial class AdminDonationHistoryViewModel : ObservableObject
{
    private readonly IDonationRepository _donationRepo;
    private readonly ICampaignRepository _campaignRepo;

    public AdminDonationHistoryViewModel(IDonationRepository donationRepo, ICampaignRepository campaignRepo)
    {
        _donationRepo = donationRepo;
        _campaignRepo = campaignRepo;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private IReadOnlyList<DonationDto> _donations = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var donations = await _donationRepo.GetAdminHistoryAsync();
            if (donations.Count > 0)
            {
                var campaigns = await _campaignRepo.GetActiveForDonorsAsync(1, 1000);
                var campaignMap = campaigns.ToDictionary(c => c.Id, c => c.Title);
                if (donations.Any(d => !campaignMap.ContainsKey(d.CampaignId)))
                {
                    campaigns = await _campaignRepo.GetActiveForDonorsAsync(1, 1000);
                    campaignMap = campaigns.ToDictionary(c => c.Id, c => c.Title);
                }
                donations = donations
                    .Select(d => d with { CampaignTitle = campaignMap.GetValueOrDefault(d.CampaignId) })
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
            Donations = donations;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message.Contains("403")
                ? "Apenas gestores podem acessar o histórico administrativo."
                : ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
