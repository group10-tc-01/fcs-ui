using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fcs.UI.Application.Commands;
using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Queries;
using MediatR;
using System.Collections.ObjectModel;

namespace Fcs.UI.Application.ViewModels;

public sealed partial class AdminCampaignListViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public ObservableCollection<CampaignDto> Campaigns { get; } = [];

    public AdminCampaignListViewModel(IMediator mediator)
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
            var result = await _mediator.Send(new GetAdminCampaignsQuery());
            if (result.IsFailed)
            {
                ErrorMessage = result.Errors.First().Message;
                return;
            }

            Campaigns.Clear();
            foreach (var campaign in result.Value)
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

    [RelayCommand]
    private async Task CompleteAsync(Guid id)
    {
        var result = await _mediator.Send(new CompleteCampaignCommand(id));

        if (result.IsFailed)
            ErrorMessage = result.Errors.First().Message;
        else
            await LoadAsync();
    }

    [RelayCommand]
    private async Task CancelAsync(Guid id)
    {
        var result = await _mediator.Send(new CancelCampaignCommand(id));

        if (result.IsFailed)
            ErrorMessage = result.Errors.First().Message;
        else
            await LoadAsync();
    }
}
