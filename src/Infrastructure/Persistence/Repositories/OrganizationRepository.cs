using Application.Features.Organizations;
using Application.Features.Organizations.DataTransferObjects.Responses;
using Application.Features.Organizations.Mappers;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository(ChatDb db) : IOrganizationRepository
{
    public async Task<IReadOnlyList<Organization>> GetAllWithDomainsAsync(CancellationToken cancellationToken = default) =>
        await db.Organizations.Include(organization => organization.Domains).ToListAsync(cancellationToken);

    public Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default) =>
        db.Organizations.AddAsync(organization, cancellationToken).AsTask();

    public void RemoveDomain(OrganizationDomain domain) => db.OrganizationDomains.Remove(domain);

    public Task RemoveMembersAsync(int organizationId, CancellationToken cancellationToken = default) =>
        db.OrganizationMembers.Where(member => member.OrganizationId == organizationId).ExecuteDeleteAsync(cancellationToken);

    public Task<Organization?> FindActiveByDomainAsync(string domain, CancellationToken cancellationToken = default) =>
        db.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(organization => organization.IsActive && organization.Domains.Any(x => x.Domain == domain), cancellationToken);

    public Task<OrganizationMember?> GetMembershipAsync(int userId, CancellationToken cancellationToken = default) =>
        db.OrganizationMembers.Include(member => member.Organization)
            .FirstOrDefaultAsync(member => member.UserId == userId, cancellationToken);

    public async Task<bool> TryAddMemberAsync(OrganizationMember member, CancellationToken cancellationToken = default)
    {
        var entry = await db.OrganizationMembers.AddAsync(member, cancellationToken);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            entry.State = EntityState.Detached;
            return false;
        }
    }

    public void RemoveMember(OrganizationMember member) => db.OrganizationMembers.Remove(member);

    public async Task<IReadOnlyList<int>> GetAutoJoinGroupChatIdsAsync(int organizationId, CancellationToken cancellationToken = default) =>
        await db.Groups.AsNoTracking()
            .Where(group => group.OrganizationId == organizationId && group.AutoJoin && group.Chat.DeletedAt == null)
            .OrderBy(group => group.Id)
            .Select(group => group.ChatId)
            .ToListAsync(cancellationToken);

    public Task<MyOrganizationDto?> GetMyOrganizationAsync(int userId, CancellationToken cancellationToken = default) =>
        db.OrganizationMembers.AsNoTracking()
            .Where(member => member.UserId == userId && member.Organization.IsActive)
            .Select(OrganizationMapper.MyOrganizationProjection)
            .FirstOrDefaultAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}
