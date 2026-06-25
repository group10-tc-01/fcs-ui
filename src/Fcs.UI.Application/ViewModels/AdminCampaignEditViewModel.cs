using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fcs.UI.Application.Commands;
using Fcs.UI.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace Fcs.UI.Application.ViewModels;

public sealed partial class AdminCampaignEditViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigation;
    private Guid _editingId;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddMonths(1);

    [ObservableProperty]
    private decimal _financialGoal;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isSubmitting;

    [ObservableProperty]
    private bool _isLoading;

    public bool IsEditing => _editingId != Guid.Empty;
    public string TitleText => IsEditing ? "Editar Campanha" : "Nova Campanha";

    public AdminCampaignEditViewModel(IMediator mediator, NavigationManager navigation)
    {
        _mediator = mediator;
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task LoadAsync(Guid id)
    {
        IsLoading = true;
        _editingId = id;

        try
        {
            var result = await _mediator.Send(new GetAdminCampaignsQuery(PageSize: 100));
            if (result.IsFailed)
            {
                ErrorMessage = result.Errors.First().Message;
                return;
            }

            var campaign = result.Value.FirstOrDefault(c => c.Id == id);
            if (campaign is null)
            {
                ErrorMessage = "Campanha não encontrada";
                return;
            }

            Title = campaign.Title;
            Description = campaign.Description;
            StartDate = campaign.StartDate;
            EndDate = campaign.EndDate;
            FinancialGoal = campaign.FinancialGoal;
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
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "O título é obrigatório";
            return;
        }

        if (FinancialGoal <= 0)
        {
            ErrorMessage = "A meta financeira deve ser maior que zero";
            return;
        }

        if (EndDate <= StartDate)
        {
            ErrorMessage = "A data de término deve ser posterior à data de início";
            return;
        }

        IsSubmitting = true;
        ErrorMessage = null;

        try
        {
            if (IsEditing)
            {
                var result = await _mediator.Send(
                    new UpdateCampaignCommand(_editingId, Title, Description, StartDate, EndDate, FinancialGoal));

                if (result.IsFailed)
                {
                    ErrorMessage = result.Errors.First().Message;
                    return;
                }
            }
            else
            {
                var result = await _mediator.Send(
                    new CreateCampaignCommand(Title, Description, StartDate, EndDate, FinancialGoal));

                if (result.IsFailed)
                {
                    ErrorMessage = result.Errors.First().Message;
                    return;
                }
            }

            _navigation.NavigateTo("/admin/campaigns");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
