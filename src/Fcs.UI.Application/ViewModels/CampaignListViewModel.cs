using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Queries;
using MediatR;
using System.Collections.ObjectModel;

namespace Fcs.UI.Application.ViewModels;

public sealed partial class CampaignListViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public ObservableCollection<CampaignDto> Campaigns { get; } = [];

    public CampaignListViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var result = await _mediator.Send(new GetCampaignsQuery());

            if (result.IsFailed)
            {
                ErrorMessage = result.Errors.First().Message;
                return;
            }

            Campaigns.Clear();
            foreach (var campaign in result.Value
                .OrderByDescending(c => c.FinancialGoal > 0 ? c.TotalAmountRaised / c.FinancialGoal : 0))
                Campaigns.Add(campaign);
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
}
