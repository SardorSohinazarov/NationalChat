using Application.Features.Organizations;
using Application.Features.Organizations.DataTransferObjects.Responses;
using Application.Features.Organizations.Mappers;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository(ChatDb db) : IOrganizationRepository
{
    public Task<Organization?> FindByDomainAsync(string domain, CancellationToken cancellationToken = default) =>
        db.Organizations.FirstOrDefaultAsync(organization => organization.Domain == domain, cancellationToken);

    public Task<bool> TryAddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default) =>
        TryAddAsync(organization, cancellationToken);

    public Task<OrganizationMember?> GetMembershipAsync(int userId, CancellationToken cancellationToken = default) =>
        db.OrganizationMembers.AsNoTracking().FirstOrDefaultAsync(member => member.UserId == userId, cancellationToken);

    public Task<bool> TryAddMemberAsync(OrganizationMember member, CancellationToken cancellationToken = default) =>
        TryAddAsync(member, cancellationToken);

    public async Task<IReadOnlyList<int>> GetOrganizationGroupChatIdsAsync(int organizationId, CancellationToken cancellationToken = default) =>
        await db.Groups.AsNoTracking()
            .Where(group => group.OrganizationId == organizationId && group.Chat.DeletedAt == null)
            .OrderBy(group => group.Id)
            .Select(group => group.ChatId)
            .ToListAsync(cancellationToken);

    public Task<MyOrganizationDto?> GetMyOrganizationAsync(int userId, CancellationToken cancellationToken = default) =>
        db.OrganizationMembers.AsNoTracking()
            .Where(member => member.UserId == userId)
            .Select(OrganizationMapper.MyOrganizationProjection)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>Unique indexes (domain, user) decide races between simultaneous first sign-ins.</summary>
    private async Task<bool> TryAddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : class
    {
        var entry = await db.Set<TEntity>().AddAsync(entity, cancellationToken);
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
}
