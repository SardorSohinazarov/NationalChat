using Application.Features.Messages.Factories;
using Domain.Entities;

namespace Application.Features.Groups.Factories;

public static class GroupFactory
{
    /// <summary>
    /// Creates the group chat aggregate: chat, group metadata, memberships and the initial
    /// "group created" service message, which also makes the chat visible in everyone's chat list.
    /// </summary>
    public static Group Create(string title, string? description, User creator, IReadOnlyCollection<User> members, DateTime createdAt)
    {
        var chat = new Chat
        {
            Type = ChatType.Group,
            CreatedAt = createdAt,
            Members = [CreateMember(creator, ChatMemberRole.Creator, createdAt), .. members.Select(member => CreateMember(member, ChatMemberRole.Member, createdAt))]
        };

        var createdMessage = MessageFactory.CreateService(0, creator.Id, MessageServiceAction.GroupCreated, title, createdAt);
        createdMessage.Sender = creator;
        chat.Messages.Add(createdMessage);

        var group = new Group
        {
            Chat = chat,
            Title = title,
            Description = description,
            CreatorId = creator.Id,
            Creator = creator
        };
        chat.Groups.Add(group);
        return group;
    }

    public static ChatMember CreateMember(User user, ChatMemberRole role, DateTime joinedAt) =>
        new() { UserId = user.Id, User = user, Role = role, JoinedAt = joinedAt };
}
