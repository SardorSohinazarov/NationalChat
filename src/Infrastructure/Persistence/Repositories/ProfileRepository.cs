using Application.Features.Profiles;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ProfileRepository(ChatDb db) : BaseRepository<User>(db), IProfileRepository
{
    public Task<User?> GetUserAsync(int userId, CancellationToken cancellationToken) =>
        Db.Users.Include(x => x.ProfilePhoto).ThenInclude(photo => photo!.File).FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public Task<bool> UsernameExistsAsync(string username, int excludedUserId, CancellationToken cancellationToken) =>
        Db.Users.AnyAsync(x => x.Username == username && x.Id != excludedUserId, cancellationToken);

    public Task<Photo?> GetPhotoAsync(int photoId, CancellationToken cancellationToken) =>
        Db.Photos.AsNoTracking().Include(photo => photo.File).FirstOrDefaultAsync(photo => photo.Id == photoId, cancellationToken);

    public async Task AddPhotoAsync(Domain.Entities.File file, Photo photo, CancellationToken cancellationToken)
    {
        await Db.Set<Domain.Entities.File>().AddAsync(file, cancellationToken);
        await Db.Photos.AddAsync(photo, cancellationToken);
    }

    public async Task<IReadOnlyCollection<int>> GetRelatedUserIdsAsync(int userId, CancellationToken cancellationToken) =>
        await Db.ChatMembers.AsNoTracking()
            .Where(member => member.UserId != userId && member.Chat.Members.Any(other => other.UserId == userId))
            .Select(member => member.UserId)
            .Distinct()
            .ToArrayAsync(cancellationToken);
}
