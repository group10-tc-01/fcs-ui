using Fcs.UI.Application.DTOs;
using Refit;

namespace Fcs.UI.Infrastructure.Http;

public interface ICampaignApi
{
    [Get("/api/v1/campaigns")]
    Task<ApiEnvelope<List<CampaignDto>>> GetAllAsync(int page = 1, int pageSize = 10);

    [Get("/api/v1/campaigns/{id}")]
    Task<ApiEnvelope<CampaignDto>> GetByIdAsync(Guid id);

    [Get("/api/v1/campaigns/active")]
    Task<ApiEnvelope<List<ActiveDonorCampaignDto>>> GetActiveForDonorsAsync(int page = 1, int pageSize = 10);

    [Get("/api/v1/transparency/campaigns")]
    Task<ApiEnvelope<List<CampaignDto>>> GetActiveAsync(int page = 1, int pageSize = 10);

    [Post("/api/v1/campaigns")]
    Task<ApiEnvelope<CampaignDto>> CreateAsync([Body] CreateCampaignRequest request);

    [Put("/api/v1/campaigns/{id}")]
    Task<ApiEnvelope<CampaignDto>> UpdateAsync(Guid id, [Body] UpdateCampaignRequest request);

    [Patch("/api/v1/campaigns/{id}/complete")]
    Task<ApiEnvelope<CampaignDto>> CompleteAsync(Guid id);

    [Patch("/api/v1/campaigns/{id}/cancel")]
    Task<ApiEnvelope<CampaignDto>> CancelAsync(Guid id);
}
