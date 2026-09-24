using Application.Features.Organizations;
using Application.Features.Organizations.Options;
using Domain.Entities;
using NationalChat.Tests.Support;

namespace NationalChat.Tests.Organizations;

public sealed class OrganizationSyncServiceTests
{
    private readonly FakeOrganizationRepository _repository = new(new FakeGroupRepository());

    private Task<IReadOnlyList<string>> SyncAsync(params OrganizationOptions[] organizations) =>
        new OrganizationSyncService(_repository, new OrganizationCatalog(organizations), new FixedTimeProvider(new DateTimeOffset(TestData.Start)))
            .SyncAsync();

    [Fact]
    public async Task CreatesOrganizationsWithNormalizedDomains()
    {
        var warnings = await SyncAsync(new OrganizationOptions { Name = "TATU", ShortName = "TATU", Domains = ["TUIT.uz.", "student.tuit.uz"] });

        Assert.Empty(warnings);
        var organization = Assert.Single(_repository.Organizations);
        Assert.True(organization.IsActive);
        Assert.Equal(["tuit.uz", "student.tuit.uz"], organization.Domains.Select(x => x.Domain));
    }

    [Fact]
    public async Task RejectsPublicAndInvalidDomains()
    {
        var warnings = await SyncAsync(new OrganizationOptions { Name = "TATU", ShortName = "TATU", Domains = ["gmail.com", "Mail.Ru", "localhost", "tuit.uz"] });

        Assert.Equal(3, warnings.Count);
        Assert.Equal(["tuit.uz"], Assert.Single(_repository.Organizations).Domains.Select(x => x.Domain));
    }

    [Fact]
    public async Task DomainCannotBelongToTwoOrganizations()
    {
        var warnings = await SyncAsync(
            new OrganizationOptions { Name = "TATU", ShortName = "TATU", Domains = ["tuit.uz"] },
            new OrganizationOptions { Name = "Boshqa", ShortName = "BOSH", Domains = ["tuit.uz", "bosh.uz"] });

        Assert.Single(warnings);
        Assert.Equal(["bosh.uz"], _repository.Organizations.Single(x => x.ShortName == "BOSH").Domains.Select(x => x.Domain));
    }

    [Fact]
    public async Task RemovedOrganization_IsDeactivatedAndLosesMembersAndDomains()
    {
        var old = TestData.Organization(1, "OLD", "old.uz");
        _repository.Organizations.Add(old);
        _repository.Members.Add(new OrganizationMember { Id = 1, OrganizationId = old.Id, Organization = old, UserId = 7, Role = OrganizationRole.Member });

        await SyncAsync(new OrganizationOptions { Name = "TATU", ShortName = "TATU", Domains = ["tuit.uz"] });

        Assert.False(old.IsActive);
        Assert.Empty(old.Domains);
        Assert.Empty(_repository.Members);
        Assert.True(_repository.Organizations.Single(x => x.ShortName == "TATU").IsActive);
    }

    [Fact]
    public async Task ExistingOrganization_IsUpdatedInPlace()
    {
        var tatu = TestData.Organization(1, "TATU", "tuit.uz", "old.tuit.uz");
        tatu.IsActive = false;
        _repository.Organizations.Add(tatu);

        await SyncAsync(new OrganizationOptions { Name = "Muhammad al-Xorazmiy nomidagi TATU", ShortName = "TATU", Domains = ["tuit.uz", "student.tuit.uz"] });

        var organization = Assert.Single(_repository.Organizations);
        Assert.Same(tatu, organization);
        Assert.True(organization.IsActive);
        Assert.Equal("Muhammad al-Xorazmiy nomidagi TATU", organization.Name);
        Assert.Equal(["tuit.uz", "student.tuit.uz"], organization.Domains.Select(x => x.Domain));
    }
}
