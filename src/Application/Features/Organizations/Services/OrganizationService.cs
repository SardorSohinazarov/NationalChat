using Application.Features.Organizations.DataTransferObjects.Responses;

namespace Application.Features.Organizations;

public sealed class OrganizationService(IOrganizationRepository repository) : IOrganizationService
{
    public Task<MyOrganizationDto?> GetMineAsync(int userId, CancellationToken cancellationToken = default) =>
        repository.GetMyOrganizationAsync(userId, cancellationToken);
}
