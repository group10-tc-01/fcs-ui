using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using Fcs.UI.Domain.Entities;
using Fcs.UI.Infrastructure.Http;

namespace Fcs.UI.Infrastructure.Services;

public sealed class DonationRepository : IDonationRepository
{
    private readonly IDonationApi _donationApi;

    public DonationRepository(IDonationApi donationApi)
    {
        _donationApi = donationApi;
    }

    public async Task AddAsync(Donation donation)
    {
        var request = new DonationRequest(donation.CampaignId, donation.Amount);
        var envelope = await _donationApi.SubmitAsync(request);

        if (envelope is { Success: true, Data: not null })
        {
            donation.Confirm();
        }
        else
        {
            donation.Fail();
        }
    }

    public Task<Donation?> GetByIdAsync(Guid id)
    {
        return Task.FromResult<Donation?>(null);
    }

    public async Task<IReadOnlyList<Donation>> GetByCampaignAsync(Guid campaignId)
    {
        var donations = await _donationApi.GetHistoryAsync();
        if (donations is null)
            return Array.Empty<Donation>();

        return donations
            .Where(d => d.CampaignId == campaignId)
            .Select(d => new Donation(d.CampaignId, d.Amount, ""))
            .ToList();
    }

    public async Task<IReadOnlyList<DonationDto>> GetHistoryAsync()
    {
        return await _donationApi.GetHistoryAsync() ?? [];
    }

    public async Task<IReadOnlyList<DonationDto>> GetAdminHistoryAsync()
    {
        return await _donationApi.GetAdminHistoryAsync() ?? [];
    }
}
