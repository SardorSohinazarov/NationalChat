using System.Linq.Expressions;
using Application.Features.Chats.DataTransferObjects.Responses;
using Application.Features.Organizations.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Chats.Mappers;

public static class ChatListMapper
{
    public static Expression<Func<Chat, ChatListDto>> Projection(int currentUserId) => chat =>
        new(
            chat.Id,
            chat.Type,
            chat.CreatedAt,
            chat.Type == ChatType.Private
                ? chat.Members.Where(member => member.UserId != currentUserId)
                    .Select(member => new PrivateChatParticipantDto(
                        member.User.Id, member.User.Username, member.User.FirstName, member.User.LastName, member.User.ProfilePhotoId,
                        false,
                        member.User.Sessions.Where(session => session.RevokedAt == null).Select(session => (DateTime?)session.LastActiveAt).Max(),
                        member.User.OrganizationMembership == null ? null : new OrganizationBadgeDto(member.User.OrganizationMembership.Organization.Id, member.User.OrganizationMembership.Organization.Domain)))
                    .FirstOrDefault()
                : null,
            chat.Type == ChatType.Group
                ? chat.Groups
                    .Select(group => new GroupChatSummaryDto(group.Title, group.PhotoId, chat.Members.Count))
                    .FirstOrDefault()
                : null,
            chat.Messages.OrderByDescending(message => message.Id)
                .Select(message => new ChatLastMessageDto(
                    message.Id, message.TextContent, message.SentAt, message.SenderId,
                    message.Sender.Username, message.Sender.FirstName, message.ServiceAction))
                .FirstOrDefault(),
            chat.Messages.Count(message => message.SenderId != currentUserId && !message.Views.Any(view => view.UserId == currentUserId)));
}
