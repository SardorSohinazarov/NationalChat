using Application.Features.Files;
using Application.Features.Files.DataTransferObjects.Requests;
using Application.Features.Messages;
using Application.Features.Profiles.DataTransferObjects.Requests;
using Application.Features.Profiles.DataTransferObjects.Responses;
using Application.Features.Profiles.Mappers;
using Domain.Entities;
using FluentValidation;

namespace Application.Features.Profiles;

public sealed class ProfileService(
    IProfileRepository store,
    IValidator<UpdateProfileRequest> updateProfileValidator,
    IFileService fileService,
    IChatRealtimeNotifier realtimeNotifier) : IProfileService
{
    public async Task<ProfileDto?> GetMyProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await store.GetUserAsync(userId, cancellationToken);
        return user is null ? null : ProfileMapper.ToDto(user);
    }

    public async Task<ProfileDto?> UpdateMyProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await updateProfileValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return null;
        }

        var username = NormalizeUsername(request.Username)!;

        var user = await store.GetUserAsync(userId, cancellationToken);
        if (user is null || (username != user.Username && await store.UsernameExistsAsync(username, userId, cancellationToken)))
        {
            return null;
        }

        user.Username = username;
        user.FirstName = request.FirstName.Trim();
        user.LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName.Trim();
        user.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim()[..Math.Min(request.Bio.Trim().Length, 255)];
        await store.SaveChangesAsync(cancellationToken);
        return ProfileMapper.ToDto(user);
    }

    public async Task<ProfilePhotoUpdateResult> UpdateMyPhotoAsync(int userId, StoreImageRequest request, CancellationToken cancellationToken = default)
    {
        var storedResult = await fileService.StoreImageAsync(request, cancellationToken);
        if (storedResult.File is null) return new(null, storedResult.Error);
        var stored = storedResult.File;

        var user = await store.GetUserAsync(userId, cancellationToken);
        if (user is null)
        {
            await fileService.DeleteAsync(stored.StoragePath, cancellationToken);
            return new(null, "User aniqlanmadi.");
        }

        var oldFile = user.ProfilePhoto?.File;
        var file = new Domain.Entities.File { Name = stored.FileName, MimeType = stored.MimeType, SizeBytes = stored.SizeBytes, StoragePath = stored.StoragePath };
        var photo = new Photo { File = file, Width = stored.Width, Height = stored.Height };
        await store.AddPhotoAsync(file, photo, cancellationToken);
        user.ProfilePhoto = photo;
        await store.SaveChangesAsync(cancellationToken);

        if (oldFile is not null) await fileService.DeleteAsync(oldFile.StoragePath, cancellationToken);

        var relatedUserIds = await store.GetRelatedUserIdsAsync(userId, cancellationToken);
        await realtimeNotifier.ProfilePhotoUpdatedAsync(userId, user.ProfilePhotoId, relatedUserIds, cancellationToken);

        return new(ProfileMapper.ToDto(user), null);
    }

    public async Task<ProfilePhotoContent?> GetPhotoAsync(int photoId, bool original = false, CancellationToken cancellationToken = default)
    {
        var photo = await store.GetPhotoAsync(photoId, cancellationToken);
        if (photo?.File is null) return null;

        if (!original)
        {
            var thumbnailStream = await fileService.OpenReadAsync(ThumbnailPaths.ForOriginal(photo.File.StoragePath), cancellationToken);
            if (thumbnailStream is not null) return new(thumbnailStream, photo.File.MimeType);
        }

        var stream = await fileService.OpenReadAsync(photo.File.StoragePath, cancellationToken);
        return stream is null ? null : new(stream, photo.File.MimeType);
    }

    private static string? NormalizeUsername(string username)
    {
        var normalized = username.Trim().ToLowerInvariant();
        return normalized.Length is >= 5 and <= 50 && normalized.All(x => char.IsAsciiLetterOrDigit(x) || x == '_')
            ? normalized
            : null;
    }
}
