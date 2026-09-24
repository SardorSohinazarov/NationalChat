using Application.Features.Groups.DataTransferObjects.Responses;
using Application.Features.Organizations.Mappers;
using Domain.Entities;

namespace Application.Features.Groups.Mappers;

public static class GroupMapper
{
    public static GroupDto ToDto(Group group, int currentUserId, Func<int, bool> isOnline)
    {
        var members = group.Chat.Members
            .OrderByDescending(member => member.Role)
            .ThenBy(member => member.JoinedAt)
            .ThenBy(member => member.Id)
            .Select(member => ToDto(member, isOnline(member.UserId)))
            .ToArray();
        var myRole = group.Chat.Members.First(member => member.UserId == currentUserId).Role;

        return new(group.ChatId, group.Title, group.Description, group.PhotoId, group.CreatorId, myRole, group.Chat.CreatedAt, members,
            OrganizationMapper.ToBadge(group.Organization), group.AutoJoin);
    }

    public static GroupMemberDto ToDto(ChatMember member, bool isOnline) =>
        new(
            member.User.Id,
            member.User.Username,
            member.User.FirstName,
            member.User.LastName,
            member.User.ProfilePhotoId,
            member.Role,
            isOnline,
            member.User.Sessions.Where(session => session.RevokedAt == null).Select(session => (DateTime?)session.LastActiveAt).Max(),
            member.JoinedAt,
            OrganizationMapper.ToBadge(member.User));

    public static string DisplayName(User user) =>
        string.IsNullOrWhiteSpace(user.LastName) ? user.FirstName : $"{user.FirstName} {user.LastName}";
}
