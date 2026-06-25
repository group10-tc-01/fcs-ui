using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using Fcs.UI.Infrastructure.Http;

namespace Fcs.UI.Infrastructure.Services;

public sealed class CampaignRepository : ICampaignRepository
{
    private readonly ICampaignApi _api;
    private List<CampaignDto>? _activeCache;

    public CampaignRepository(ICampaignApi api)
    {
        _api = api;
    }

    public async Task<IReadOnlyList<CampaignDto>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var envelope = await _api.GetAllAsync(page, pageSize);
        return envelope?.Data ?? [];
    }

    public async Task<CampaignDto?> GetByIdAsync(Guid id)
    {
        var envelope = await _api.GetByIdAsync(id);
        return envelope?.Data;
    }

    public async Task<IReadOnlyList<CampaignDto>> GetActiveAsync(int page = 1, int pageSize = 10, bool forceRefresh = false)
    {
        if (_activeCache is not null && !forceRefresh)
            return _activeCache;

        var envelope = await _api.GetActiveAsync(page, pageSize);
        _activeCache = envelope?.Data ?? [];
        return _activeCache;
    }

    public async Task<IReadOnlyList<ActiveDonorCampaignDto>> GetActiveForDonorsAsync(int page = 1, int pageSize = 10)
    {
        var envelope = await _api.GetActiveForDonorsAsync(page, pageSize);
        return envelope?.Data ?? [];
    }

    public async Task<CampaignDto?> CreateAsync(CreateCampaignRequest request)
    {
        var envelope = await _api.CreateAsync(request);
        if (envelope?.Success == true) _activeCache = null;
        return envelope?.Data;
    }

    public async Task<CampaignDto?> UpdateAsync(Guid id, UpdateCampaignRequest request)
    {
        var envelope = await _api.UpdateAsync(id, request);
        if (envelope?.Success == true) _activeCache = null;
        return envelope?.Data;
    }

    public async Task<CampaignDto?> CompleteAsync(Guid id)
    {
        var envelope = await _api.CompleteAsync(id);
        if (envelope?.Success == true) _activeCache = null;
        return envelope?.Data;
    }

    public async Task<CampaignDto?> CancelAsync(Guid id)
    {
        var envelope = await _api.CancelAsync(id);
        if (envelope?.Success == true) _activeCache = null;
        return envelope?.Data;
    }

    public void InvalidateActiveCache() => _activeCache = null;
}
