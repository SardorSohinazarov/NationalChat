using Application.Features.Users.DataTransferObjects.Requests;
using Application.Features.Users.DataTransferObjects.Responses;

namespace Application.Features.Users;

public interface IUserDiscoveryService
{
    Task<IReadOnlyList<UserSearchDto>> SearchAsync(int currentUserId, UserSearchRequest request, CancellationToken cancellationToken = default);
}
