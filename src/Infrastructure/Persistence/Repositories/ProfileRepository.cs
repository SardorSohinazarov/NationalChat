using Application.Features.Profiles;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ProfileRepository(ChatDb db) : BaseRepository<User>(db), IProfileRepository
{
    public Task<User?> GetUserAsync(int userId, CancellationToken cancellationToken) =>
        Db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public Task<bool> UsernameExistsAsync(string username, int excludedUserId, CancellationToken cancellationToken) =>
        Db.Users.AnyAsync(x => x.Username == username && x.Id != excludedUserId, cancellationToken);

}
