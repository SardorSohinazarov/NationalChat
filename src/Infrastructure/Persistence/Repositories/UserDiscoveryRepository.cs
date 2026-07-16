using Application.Features.Users;
using Application.Features.Users.DataTransferObjects.Responses;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserDiscoveryRepository(ChatDb db) : IUserDiscoveryRepository
{
    public async Task<IReadOnlyList<UserSearchDto>> SearchAsync(int currentUserId, string query, int limit, CancellationToken cancellationToken = default) =>
        await db.Users.AsNoTracking()
            .Where(x => x.Id != currentUserId && x.IsProfileCompleted &&
                (EF.Functions.ILike(x.Username, $"{query}%") ||
                 EF.Functions.ILike(x.FirstName, $"{query}%") ||
                 (x.LastName != null && EF.Functions.ILike(x.LastName, $"{query}%"))))
            .OrderBy(x => x.Username)
            .Take(limit)
            .Select(x => new UserSearchDto(x.Id, x.Username, x.FirstName, x.LastName, x.ProfilePhotoId))
            .ToListAsync(cancellationToken);
}
