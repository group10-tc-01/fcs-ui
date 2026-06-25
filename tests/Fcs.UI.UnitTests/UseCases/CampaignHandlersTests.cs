using Fcs.UI.Application.Commands;
using Fcs.UI.Application.DTOs;
using Fcs.UI.Application.Interfaces;
using Fcs.UI.Application.Queries;
using FluentAssertions;
using Moq;

namespace Fcs.UI.UnitTests.UseCases;

public class CampaignHandlersTests
{
    [Fact]
    public async Task CreateCampaignHandler_Success_ReturnsCampaign()
    {
        var repo = new Mock<ICampaignRepository>();
        var expected = new CampaignDto(Guid.NewGuid(), "Camp", "Desc", DateTime.Today, DateTime.Today.AddMonths(1), 1000m, "Active", 0m, Guid.Empty, DateTime.UtcNow, null);
        repo.Setup(r => r.CreateAsync(It.IsAny<CreateCampaignRequest>())).ReturnsAsync(expected);

        var handler = new CreateCampaignHandler(repo.Object);
        var cmd = new CreateCampaignCommand("Camp", "Desc", DateTime.Today, DateTime.Today.AddMonths(1), 1000m);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public async Task CreateCampaignHandler_Failure_ReturnsFail()
    {
        var repo = new Mock<ICampaignRepository>();
        repo.Setup(r => r.CreateAsync(It.IsAny<CreateCampaignRequest>())).ReturnsAsync(null as CampaignDto);

        var handler = new CreateCampaignHandler(repo.Object);
        var cmd = new CreateCampaignCommand("Camp", "Desc", DateTime.Today, DateTime.Today.AddMonths(1), 1000m);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Falha ao criar campanha");
    }

    [Fact]
    public async Task UpdateCampaignHandler_Success_ReturnsCampaign()
    {
        var repo = new Mock<ICampaignRepository>();
        var id = Guid.NewGuid();
        var expected = new CampaignDto(id, "Updated", "Desc", DateTime.Today, DateTime.Today.AddMonths(1), 2000m, "Active", 0m, Guid.Empty, DateTime.UtcNow, null);
        repo.Setup(r => r.UpdateAsync(id, It.IsAny<UpdateCampaignRequest>())).ReturnsAsync(expected);

        var handler = new UpdateCampaignHandler(repo.Object);
        var cmd = new UpdateCampaignCommand(id, "Updated", "Desc", DateTime.Today, DateTime.Today.AddMonths(1), 2000m);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public async Task UpdateCampaignHandler_Failure_ReturnsFail()
    {
        var repo = new Mock<ICampaignRepository>();
        repo.Setup(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateCampaignRequest>())).ReturnsAsync(null as CampaignDto);

        var handler = new UpdateCampaignHandler(repo.Object);
        var cmd = new UpdateCampaignCommand(Guid.NewGuid(), "Updated", "Desc", DateTime.Today, DateTime.Today.AddMonths(1), 2000m);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Falha ao atualizar campanha");
    }

    [Fact]
    public async Task CancelCampaignHandler_Success_ReturnsCampaign()
    {
        var repo = new Mock<ICampaignRepository>();
        var id = Guid.NewGuid();
        var expected = new CampaignDto(id, "Cancelled", "", default, default, 0, "Cancelled", 0, Guid.Empty, default, null);
        repo.Setup(r => r.CancelAsync(id)).ReturnsAsync(expected);

        var handler = new CancelCampaignHandler(repo.Object);
        var cmd = new CancelCampaignCommand(id);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public async Task CancelCampaignHandler_Failure_ReturnsFail()
    {
        var repo = new Mock<ICampaignRepository>();
        repo.Setup(r => r.CancelAsync(It.IsAny<Guid>())).ReturnsAsync(null as CampaignDto);

        var handler = new CancelCampaignHandler(repo.Object);
        var result = await handler.Handle(new CancelCampaignCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Falha ao cancelar campanha");
    }

    [Fact]
    public async Task CompleteCampaignHandler_Success_ReturnsCampaign()
    {
        var repo = new Mock<ICampaignRepository>();
        var id = Guid.NewGuid();
        var expected = new CampaignDto(id, "Completed", "", default, default, 0, "Completed", 0, Guid.Empty, default, null);
        repo.Setup(r => r.CompleteAsync(id)).ReturnsAsync(expected);

        var handler = new CompleteCampaignHandler(repo.Object);
        var cmd = new CompleteCampaignCommand(id);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public async Task CompleteCampaignHandler_Failure_ReturnsFail()
    {
        var repo = new Mock<ICampaignRepository>();
        repo.Setup(r => r.CompleteAsync(It.IsAny<Guid>())).ReturnsAsync(null as CampaignDto);

        var handler = new CompleteCampaignHandler(repo.Object);
        var result = await handler.Handle(new CompleteCampaignCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Falha ao concluir campanha");
    }
}

public class CampaignQueryHandlersTests
{
    [Fact]
    public async Task GetCampaignsHandler_Success_ReturnsCampaigns()
    {
        var repo = new Mock<ICampaignRepository>();
        var campaigns = new List<CampaignDto>
        {
            new(Guid.NewGuid(), "Active", "", default, default, 0, "", 0, Guid.Empty, default, null)
        };
        repo.Setup(r => r.GetActiveAsync(1, 10)).ReturnsAsync(campaigns);

        var handler = new GetCampaignsHandler(repo.Object);
        var result = await handler.Handle(new GetCampaignsQuery(1, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCampaignsHandler_Exception_ReturnsFail()
    {
        var repo = new Mock<ICampaignRepository>();
        repo.Setup(r => r.GetActiveAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new InvalidOperationException("DB error"));

        var handler = new GetCampaignsHandler(repo.Object);
        var result = await handler.Handle(new GetCampaignsQuery(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("DB error");
    }

    [Fact]
    public async Task GetAdminCampaignsHandler_Success_ReturnsCampaigns()
    {
        var repo = new Mock<ICampaignRepository>();
        var campaigns = new List<CampaignDto>
        {
            new(Guid.NewGuid(), "Admin", "", default, default, 0, "", 0, Guid.Empty, default, null)
        };
        repo.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync(campaigns);

        var handler = new GetAdminCampaignsHandler(repo.Object);
        var result = await handler.Handle(new GetAdminCampaignsQuery(1, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAdminCampaignsHandler_Exception_ReturnsFail()
    {
        var repo = new Mock<ICampaignRepository>();
        repo.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new InvalidOperationException("DB error"));

        var handler = new GetAdminCampaignsHandler(repo.Object);
        var result = await handler.Handle(new GetAdminCampaignsQuery(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("DB error");
    }
}
