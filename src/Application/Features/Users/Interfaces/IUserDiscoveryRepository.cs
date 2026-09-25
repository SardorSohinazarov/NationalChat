using Application.Features.Users.DataTransferObjects.Responses;

namespace Application.Features.Users;

public interface IUserDiscoveryRepository
{
    Task<IReadOnlyList<UserSearchDto>> SearchAsync(int currentUserId, string query, int limit, CancellationToken cancellationToken = default);
}
