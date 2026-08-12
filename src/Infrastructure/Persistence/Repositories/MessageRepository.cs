using Application.DataTransferObjects.Pagination;
using Application.Features.Messages;
using Application.Features.Messages.DataTransferObjects.Responses;
using Application.Features.Messages.Mappers;
using Domain.Entities;
using Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class MessageRepository(ChatDb db) : IMessageRepository
{
    public Task<bool> IsChatMemberAsync(int chatId, int userId, CancellationToken cancellationToken = default) =>
        db.ChatMembers.AnyAsync(member => member.ChatId == chatId && member.UserId == userId, cancellationToken);

    public Task<bool> ExistsInChatAsync(int messageId, int chatId, CancellationToken cancellationToken = default) =>
        db.Messages.AnyAsync(message => message.Id == messageId && message.ChatId == chatId, cancellationToken);

    public Task<CursorPagedResponse<MessageDto>> GetMessagesAsync(int chatId, int currentUserId, CursorPaginationRequest pagination, CancellationToken cancellationToken = default) =>
        db.Messages.AsNoTracking()
            .Where(message => message.ChatId == chatId)
            .ToCursorPagedResponseAsync(pagination, message => message.Id, MessageMapper.Projection(currentUserId), message => message.Id, cancellationToken);

    public Task<Message?> GetByIdAsync(int messageId, CancellationToken cancellationToken = default) =>
        db.Messages.AsNoTracking()
            .Include(message => message.Sender)
            .FirstOrDefaultAsync(message => message.Id == messageId, cancellationToken);

    public async Task<IReadOnlyCollection<int>> GetMemberUserIdsAsync(int chatId, CancellationToken cancellationToken = default) =>
        await db.ChatMembers.AsNoTracking()
            .Where(member => member.ChatId == chatId)
            .Select(member => member.UserId)
            .ToArrayAsync(cancellationToken);

    public Task AddAsync(Message message, CancellationToken cancellationToken = default) =>
        db.Messages.AddAsync(message, cancellationToken).AsTask();

    public async Task MarkAsReadAsync(int chatId, int userId, IReadOnlyCollection<int> messageIds, DateTime viewedAt, CancellationToken cancellationToken = default)
    {
        if (messageIds.Count == 0)
        {
            return;
        }

        var unreadMessageIds = await db.Messages
            .Where(message => message.ChatId == chatId && message.SenderId != userId && messageIds.Contains(message.Id) &&
                !message.Views.Any(view => view.UserId == userId))
            .Select(message => message.Id)
            .ToListAsync(cancellationToken);

        if (unreadMessageIds.Count == 0)
        {
            return;
        }

        await db.MessageViews.AddRangeAsync(unreadMessageIds.Select(messageId => new MessageView
        {
            MessageId = messageId,
            UserId = userId,
            ViewedAt = viewedAt
        }), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}
