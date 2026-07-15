using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Features.Profiles;

public interface IProfileRepository : IBaseRepository<User>
{
    Task<User?> GetUserAsync(int userId, CancellationToken cancellationToken);
    Task<bool> UsernameExistsAsync(string username, int excludedUserId, CancellationToken cancellationToken);
}
