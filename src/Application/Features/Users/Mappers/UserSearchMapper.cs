using System.Linq.Expressions;
using Application.Features.Organizations.DataTransferObjects.Responses;
using Application.Features.Users.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Users.Mappers;

public static class UserSearchMapper
{
    public static Expression<Func<User, UserSearchDto>> Projection => user =>
        new(user.Id, user.Username, user.FirstName, user.LastName, user.ProfilePhotoId,
            user.OrganizationMembership == null ? null : new OrganizationBadgeDto(user.OrganizationMembership.Organization.Id, user.OrganizationMembership.Organization.ShortName));
}
