using Application.Features.Profiles.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Profiles.Mappers;

public static class ProfileMapper
{
    public static ProfileDto ToDto(User user) =>
        new(user.Id, user.Email, user.Username, user.FirstName, user.LastName, user.Bio, user.ProfilePhotoId, user.CreatedAt);
}
