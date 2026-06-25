using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using System.Collections.ObjectModel;

namespace Fcs.UI.Application.ViewModels;

public sealed partial class CampaignSelectionViewModel : ObservableObject
{
    private readonly ICampaignRepository _repo;
    private List<ActiveDonorCampaignDto> _allCampaigns = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _selectedSort = "progress";

    public ObservableCollection<ActiveDonorCampaignDto> Campaigns { get; } = [];

    public CampaignSelectionViewModel(ICampaignRepository repo)
    {
        _repo = repo;
    }

    partial void OnSelectedSortChanged(string value)
    {
        ApplySorting();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            _allCampaigns = (await _repo.GetActiveForDonorsAsync()).ToList();
            ApplySorting();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplySorting()
    {
        var sorted = SelectedSort switch
        {
            "name-asc" => _allCampaigns.OrderBy(c => c.Title).ToList(),
            "name-desc" => _allCampaigns.OrderByDescending(c => c.Title).ToList(),
            "goal-asc" => _allCampaigns.OrderBy(c => c.FinancialGoal).ToList(),
            "goal-desc" => _allCampaigns.OrderByDescending(c => c.FinancialGoal).ToList(),
            "enddate-asc" => _allCampaigns.OrderBy(c => c.EndDate).ToList(),
            "enddate-desc" => _allCampaigns.OrderByDescending(c => c.EndDate).ToList(),
            _ => _allCampaigns.OrderByDescending(c => c.FinancialGoal > 0 ? c.TotalAmountRaised / c.FinancialGoal : 0).ToList(),
        };

        Campaigns.Clear();
        foreach (var c in sorted)
            Campaigns.Add(c);
    }
}
