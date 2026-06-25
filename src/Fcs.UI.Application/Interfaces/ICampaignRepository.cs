using Fcs.UI.Application.DTOs;

namespace Fcs.UI.Application.Interfaces;

public interface ICampaignRepository
{
    Task<IReadOnlyList<CampaignDto>> GetAllAsync(int page = 1, int pageSize = 10);    
    Task<IReadOnlyList<CampaignDto>> GetActiveAsync(int page = 1, int pageSize = 10, bool forceRefresh = false);
    Task<IReadOnlyList<ActiveDonorCampaignDto>> GetActiveForDonorsAsync(int page = 1, int pageSize = 10);
    Task<CampaignDto?> GetByIdAsync(Guid id);
    Task<CampaignDto?> CreateAsync(CreateCampaignRequest request);
    Task<CampaignDto?> UpdateAsync(Guid id, UpdateCampaignRequest request);
    Task<CampaignDto?> CompleteAsync(Guid id);
    Task<CampaignDto?> CancelAsync(Guid id);
    void InvalidateActiveCache();
}
