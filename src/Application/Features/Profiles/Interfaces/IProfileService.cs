using Application.Features.Profiles.DataTransferObjects.Requests;
using Application.Features.Profiles.DataTransferObjects.Responses;

namespace Application.Features.Profiles;

public interface IProfileService
{
    Task<ProfileDto?> GetMyProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<ProfileDto?> UpdateMyProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
}
