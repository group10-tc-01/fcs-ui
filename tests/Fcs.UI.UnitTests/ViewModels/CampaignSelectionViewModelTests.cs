using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using Fcs.UI.Application.ViewModels;
using FluentAssertions;
using Moq;

namespace Fcs.UI.UnitTests.ViewModels;

public class CampaignSelectionViewModelTests
{
    [Fact]
    public async Task LoadAsync_Success_PopulatesSortedCampaigns()
    {
        var repo = new Mock<ICampaignRepository>();
        var camp1 = new ActiveDonorCampaignDto(Guid.NewGuid(), "Camp1", 1000m, 100m, default, default);
        var camp2 = new ActiveDonorCampaignDto(Guid.NewGuid(), "Camp2", 2000m, 1500m, default, default);
        var camp3 = new ActiveDonorCampaignDto(Guid.NewGuid(), "Camp3", 1000m, 0m, default, default);
        var campaigns = new List<ActiveDonorCampaignDto> { camp1, camp2, camp3 };
        repo.Setup(r => r.GetActiveForDonorsAsync(1, 10)).ReturnsAsync(campaigns);

        var vm = new CampaignSelectionViewModel(repo.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
        vm.Campaigns.Should().HaveCount(3);
        vm.Campaigns[0].Title.Should().Be("Camp2");
        vm.Campaigns[1].Title.Should().Be("Camp1");
        vm.Campaigns[2].Title.Should().Be("Camp3");
    }

    [Fact]
    public async Task LoadAsync_EmptyList_NoCrash()
    {
        var repo = new Mock<ICampaignRepository>();
        repo.Setup(r => r.GetActiveForDonorsAsync(1, 10)).ReturnsAsync([]);

        var vm = new CampaignSelectionViewModel(repo.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.Campaigns.Should().BeEmpty();
        vm.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_Exception_ShowsError()
    {
        var repo = new Mock<ICampaignRepository>();
        repo.Setup(r => r.GetActiveForDonorsAsync(1, 10)).ThrowsAsync(new InvalidOperationException("Repo error"));

        var vm = new CampaignSelectionViewModel(repo.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("Repo error");
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_WithZeroFinancialGoal_DoesNotDivideByZero()
    {
        var repo = new Mock<ICampaignRepository>();
        var camp = new ActiveDonorCampaignDto(Guid.NewGuid(), "Zero", 0m, 0m, default, default);
        repo.Setup(r => r.GetActiveForDonorsAsync(1, 10)).ReturnsAsync(new List<ActiveDonorCampaignDto> { camp });

        var vm = new CampaignSelectionViewModel(repo.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.Campaigns.Should().ContainSingle();
        vm.ErrorMessage.Should().BeNull();
    }
}

public class DonationHistoryViewModelTests
{
    private static readonly Guid CampaignA = Guid.NewGuid();
    private static readonly Guid CampaignB = Guid.NewGuid();
    private static readonly Guid DonorId = Guid.NewGuid();

    private static readonly List<DonationDto> SampleDonations =
    [
        new(Guid.NewGuid(), CampaignA, DonorId, 100m, "Processed", DateTime.UtcNow, DateTime.UtcNow, null),
        new(Guid.NewGuid(), CampaignB, DonorId, 50m, "Pending", DateTime.UtcNow, null, null),
    ];

    private static readonly List<ActiveDonorCampaignDto> SampleCampaigns =
    [
        new(CampaignA, "Campanha A", 1000m, 500m, DateTime.UtcNow, DateTime.UtcNow.AddDays(30)),
        new(CampaignB, "Campanha B", 2000m, 300m, DateTime.UtcNow, DateTime.UtcNow.AddDays(30)),
    ];

    [Fact]
    public async Task LoadAsync_Success_PopulatesDonations()
    {
        var donationRepo = new Mock<IDonationRepository>();
        donationRepo.Setup(r => r.GetHistoryAsync()).ReturnsAsync(SampleDonations);

        var campaignRepo = new Mock<ICampaignRepository>();
        campaignRepo.Setup(r => r.GetActiveForDonorsAsync(1, 1000)).ReturnsAsync(SampleCampaigns);

        var vm = new DonationHistoryViewModel(donationRepo.Object, campaignRepo.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
        vm.Donations.Should().HaveCount(2);
        vm.Donations.Should().Contain(d => d.CampaignTitle == "Campanha A");
        vm.Donations.Should().Contain(d => d.CampaignTitle == "Campanha B");
        vm.Donations[0].CreatedAt.Should().BeOnOrAfter(vm.Donations[1].CreatedAt);
    }

    [Fact]
    public async Task LoadAsync_DonationRepoException_ShowsError()
    {
        var donationRepo = new Mock<IDonationRepository>();
        donationRepo.Setup(r => r.GetHistoryAsync()).ThrowsAsync(new InvalidOperationException("API error"));

        var vm = new DonationHistoryViewModel(donationRepo.Object, new Mock<ICampaignRepository>().Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("API error");
        vm.IsLoading.Should().BeFalse();
        vm.Donations.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_CampaignRepoException_StillShowsDonations()
    {
        var donationRepo = new Mock<IDonationRepository>();
        donationRepo.Setup(r => r.GetHistoryAsync()).ReturnsAsync(SampleDonations);

        var campaignRepo = new Mock<ICampaignRepository>();
        campaignRepo.Setup(r => r.GetActiveForDonorsAsync(1, 1000)).ThrowsAsync(new InvalidOperationException("Campaign error"));

        var vm = new DonationHistoryViewModel(donationRepo.Object, campaignRepo.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("Campaign error");
        vm.IsLoading.Should().BeFalse();
        vm.Donations.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_NoDonations_SkipsCampaignFetch()
    {
        var donationRepo = new Mock<IDonationRepository>();
        donationRepo.Setup(r => r.GetHistoryAsync()).ReturnsAsync([]);

        var campaignRepo = new Mock<ICampaignRepository>(MockBehavior.Strict);

        var vm = new DonationHistoryViewModel(donationRepo.Object, campaignRepo.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
        vm.Donations.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_Forbidden403_ShowsFriendlyMessage()
    {
        var donationRepo = new Mock<IDonationRepository>();
        donationRepo.Setup(r => r.GetHistoryAsync())
            .ThrowsAsync(new InvalidOperationException("Response status code does not indicate success: 403 (Forbidden)."));

        var vm = new DonationHistoryViewModel(donationRepo.Object, new Mock<ICampaignRepository>().Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("Apenas doadores podem acessar o histórico de doações.");
        vm.IsLoading.Should().BeFalse();
        vm.Donations.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_MissingCampaign_RefreshesCache()
    {
        var unknownCampaignId = Guid.NewGuid();
        var donations = new List<DonationDto>
        {
            new(Guid.NewGuid(), unknownCampaignId, DonorId, 100m, "Processed", DateTime.UtcNow, DateTime.UtcNow, null),
        };

        var donationRepo = new Mock<IDonationRepository>();
        donationRepo.Setup(r => r.GetHistoryAsync()).ReturnsAsync(donations);

        var campaignRepo = new Mock<ICampaignRepository>();
        campaignRepo.Setup(r => r.GetActiveForDonorsAsync(1, 1000)).ReturnsAsync([]);
        campaignRepo.Setup(r => r.GetByIdAsync(unknownCampaignId)).ReturnsAsync(
            new CampaignDto(unknownCampaignId, "Campanha Retornada", "Descrição", default, default, 1000m, "Active", 500m, Guid.Empty, default, default));

        var vm = new DonationHistoryViewModel(donationRepo.Object, campaignRepo.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().BeNull();
        vm.Donations.Should().ContainSingle();
        vm.Donations[0].CampaignTitle.Should().Be("Campanha Retornada");
    }

    [Fact]
    public async Task LoadAsync_MissingCampaignAfterRefresh_FallsBackToUnavailable()
    {
        var unknownCampaignId = Guid.NewGuid();
        var donations = new List<DonationDto>
        {
            new(Guid.NewGuid(), unknownCampaignId, DonorId, 100m, "Processed", DateTime.UtcNow, DateTime.UtcNow, null),
        };

        var donationRepo = new Mock<IDonationRepository>();
        donationRepo.Setup(r => r.GetHistoryAsync()).ReturnsAsync(donations);

        var campaignRepo = new Mock<ICampaignRepository>();
        campaignRepo.Setup(r => r.GetActiveForDonorsAsync(1, 1000)).ReturnsAsync([]);
        campaignRepo.Setup(r => r.GetByIdAsync(unknownCampaignId)).ReturnsAsync((CampaignDto?)null);

        var vm = new DonationHistoryViewModel(donationRepo.Object, campaignRepo.Object);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().BeNull();
        vm.Donations.Should().ContainSingle();
        vm.Donations[0].CampaignTitle.Should().Be("Campanha indisponível");
    }
}
