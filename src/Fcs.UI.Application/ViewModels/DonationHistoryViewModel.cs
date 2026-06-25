using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;

namespace Fcs.UI.Application.ViewModels;

public sealed partial class DonationHistoryViewModel : ObservableObject
{
    private readonly IDonationRepository _donationRepo;
    private readonly ICampaignRepository _campaignRepo;

    public DonationHistoryViewModel(IDonationRepository donationRepo, ICampaignRepository campaignRepo)
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
            var donations = await _donationRepo.GetHistoryAsync();
            if (donations.Count > 0)
            {
                var campaigns = await _campaignRepo.GetActiveForDonorsAsync(1, 1000);
                var campaignMap = campaigns.ToDictionary(c => c.Id, c => c.Title);

                var missingIds = donations.Select(d => d.CampaignId).Distinct()
                    .Where(id => !campaignMap.ContainsKey(id)).ToList();

                foreach (var mid in missingIds)
                {
                    var c = await _campaignRepo.GetByIdAsync(mid);
                    if (c is not null) campaignMap[mid] = c.Title;
                }

                donations = donations
                    .Select(d => d with { CampaignTitle = campaignMap.GetValueOrDefault(d.CampaignId) ?? "Campanha indisponível" })
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
            Donations = donations;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message.Contains("403")
                ? "Apenas doadores podem acessar o histórico de doações."
                : ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
