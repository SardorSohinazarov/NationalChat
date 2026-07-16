using Application.Features.Users.DataTransferObjects.Requests;
using Application.Features.Users.DataTransferObjects.Responses;
using FluentValidation;

namespace Application.Features.Users;

public sealed class UserDiscoveryService(
    IUserDiscoveryRepository repository,
    IValidator<UserSearchRequest> searchRequestValidator) : IUserDiscoveryService
{
    public async Task<IReadOnlyList<UserSearchDto>> SearchAsync(
        int currentUserId,
        UserSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await searchRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return [];
        }

        return await repository.SearchAsync(currentUserId, request.Query.Trim().ToLowerInvariant(), request.Limit, cancellationToken);
    }
}
