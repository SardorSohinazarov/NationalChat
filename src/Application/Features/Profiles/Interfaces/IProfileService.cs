using Application.Features.Files.DataTransferObjects.Requests;
using Application.Features.Profiles.DataTransferObjects.Requests;
using Application.Features.Profiles.DataTransferObjects.Responses;

namespace Application.Features.Profiles;

public interface IProfileService
{
    Task<ProfileDto?> GetMyProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<ProfileDto?> UpdateMyProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<ProfilePhotoUpdateResult> UpdateMyPhotoAsync(int userId, StoreImageRequest request, CancellationToken cancellationToken = default);
    Task<ProfilePhotoContent?> GetPhotoAsync(int photoId, bool original = false, CancellationToken cancellationToken = default);
}

public sealed record ProfilePhotoUpdateResult(ProfileDto? Profile, string? Error);
public sealed record ProfilePhotoContent(Stream Content, string MimeType);
