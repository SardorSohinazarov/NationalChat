using Domain.Entities;

namespace Application.Features.Chats.Factories;

public static class PrivateChatFactory
{
    public static Chat Create(int firstUserId, int secondUserId, DateTime createdAt) =>
        new()
        {
            Type = ChatType.Private,
            CreatedAt = createdAt,
            Members =
            [
                new ChatMember { UserId = firstUserId, Role = ChatMemberRole.Member, JoinedAt = createdAt },
                new ChatMember { UserId = secondUserId, Role = ChatMemberRole.Member, JoinedAt = createdAt }
            ]
        };
}
