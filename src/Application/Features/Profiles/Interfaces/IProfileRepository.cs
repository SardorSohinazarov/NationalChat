using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Features.Profiles;

public interface IProfileRepository : IBaseRepository<User>
{
    Task<User?> GetUserAsync(int userId, CancellationToken cancellationToken);
    Task<bool> UsernameExistsAsync(string username, int excludedUserId, CancellationToken cancellationToken);
    Task<Photo?> GetPhotoAsync(int photoId, CancellationToken cancellationToken);
    Task AddPhotoAsync(Domain.Entities.File file, Photo photo, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<int>> GetRelatedUserIdsAsync(int userId, CancellationToken cancellationToken);
}
