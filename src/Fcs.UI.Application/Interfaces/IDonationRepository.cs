using Fcs.UI.Application.DTOs;
using Fcs.UI.Domain.Entities;

namespace Fcs.UI.Application.Interfaces;

public interface IDonationRepository
{
    Task AddAsync(Donation donation);
    Task<Donation?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Donation>> GetByCampaignAsync(Guid campaignId);
    Task<IReadOnlyList<DonationDto>> GetHistoryAsync();
    Task<IReadOnlyList<DonationDto>> GetAdminHistoryAsync();
}
