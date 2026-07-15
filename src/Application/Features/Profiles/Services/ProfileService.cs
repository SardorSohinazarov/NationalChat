using Application.Features.Profiles.DataTransferObjects.Requests;
using Application.Features.Profiles.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Profiles;

public sealed class ProfileService(IProfileRepository store) : IProfileService
{
    public async Task<ProfileDto?> GetMyProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await store.GetUserAsync(userId, cancellationToken);
        return user is null ? null : Map(user);
    }

    public async Task<ProfileDto?> UpdateMyProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var username = NormalizeUsername(request.Username);
        if (username is null || string.IsNullOrWhiteSpace(request.FirstName))
        {
            return null;
        }

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
        return Map(user);
    }

    private static ProfileDto Map(User user) =>
        new(user.Id, user.Email, user.Username, user.FirstName, user.LastName, user.Bio, user.ProfilePhotoId, user.CreatedAt);

    private static string? NormalizeUsername(string username)
    {
        var normalized = username.Trim().ToLowerInvariant();
        return normalized.Length is >= 5 and <= 50 && normalized.All(x => char.IsAsciiLetterOrDigit(x) || x == '_')
            ? normalized
            : null;
    }
}
