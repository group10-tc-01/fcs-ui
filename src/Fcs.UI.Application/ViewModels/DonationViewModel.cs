using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fcs.UI.Application.Commands;
using Fcs.UI.Application.Interfaces;
using MediatR;

namespace Fcs.UI.Application.ViewModels;

public sealed partial class DonationViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly IConnectivityService _connectivity;
    private readonly ICampaignRepository _campaignRepo;

    [ObservableProperty]
    private Guid _campaignId;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private string _donorCpf = string.Empty;

    [ObservableProperty]
    private string? _campaignTitle;

    [ObservableProperty]
    private bool _isOnline = true;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private bool _isSubmitting;

    [ObservableProperty]
    private bool _isSuccess;

    public DonationViewModel(IMediator mediator, IConnectivityService connectivity, ICampaignRepository campaignRepo)
    {
        _mediator = mediator;
        _connectivity = connectivity;
        _campaignRepo = campaignRepo;
        _connectivity.ConnectivityChanged += (_, connected) => IsOnline = connected;
        IsOnline = _connectivity.IsConnected;
    }

    public async Task InitializeAsync(Guid campaignId)
    {
        CampaignId = campaignId;
        try
        {
            var campaigns = await _campaignRepo.GetActiveForDonorsAsync(pageSize: 100);
            var campaign = campaigns.FirstOrDefault(c => c.Id == campaignId);
            if (campaign is not null)
                CampaignTitle = campaign.Title;
        }
        catch
        {
            CampaignTitle = "Campanha";
        }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (!IsOnline)
        {
            ErrorMessage = "Sem conexão com a internet";
            return;
        }

        IsSubmitting = true;
        ErrorMessage = null;
        SuccessMessage = null;
        IsSuccess = false;

        try
        {
            var cmd = new SubmitDonationCommand(CampaignId, Amount, DonorCpf);
            var result = await _mediator.Send(cmd);

            if (result.IsFailed)
                ErrorMessage = result.Errors.First().Message;
            else
            {
                IsSuccess = true;
                SuccessMessage = "Doação confirmada com sucesso!";
                _campaignRepo.InvalidateActiveCache();
            }
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
