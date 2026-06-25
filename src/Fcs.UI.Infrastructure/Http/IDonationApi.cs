using Fcs.UI.Application.DTOs;
using Refit;

namespace Fcs.UI.Infrastructure.Http;

public interface IDonationApi
{
    [Post("/api/v1/donations")]
    Task<ApiEnvelope<DonationDto>> SubmitAsync([Body] DonationRequest request);

    [Get("/api/v1/donations")]
    Task<List<DonationDto>> GetHistoryAsync();

    [Get("/api/v1/donations/admin")]
    Task<List<DonationDto>> GetAdminHistoryAsync();
}

public sealed record DonationRequest(Guid CampaignId, decimal Amount);
