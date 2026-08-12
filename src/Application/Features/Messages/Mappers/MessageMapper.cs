using System.Linq.Expressions;
using Application.Features.Messages.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Messages.Mappers;

public static class MessageMapper
{
    public static Expression<Func<Message, MessageDto>> Projection(int currentUserId) => message =>
        new(message.Id, message.ChatId, message.TextContent!, message.SentAt, message.EditedAt, message.ReplyToMessageId,
            message.ReplyToMessage == null ? null : new MessageReplyDto(message.ReplyToMessage.Id, message.ReplyToMessage.TextContent!, new MessageSenderDto(message.ReplyToMessage.Sender.Id, message.ReplyToMessage.Sender.Username, message.ReplyToMessage.Sender.FirstName, message.ReplyToMessage.Sender.LastName, message.ReplyToMessage.Sender.ProfilePhotoId)),
            new MessageSenderDto(message.Sender.Id, message.Sender.Username, message.Sender.FirstName, message.Sender.LastName, message.Sender.ProfilePhotoId),
            message.SenderId == currentUserId && message.Views.Any(view => view.UserId != currentUserId));

    public static MessageDto ToDto(Message message) =>
        new(message.Id, message.ChatId, message.TextContent!, message.SentAt, message.EditedAt, message.ReplyToMessageId,
            message.ReplyToMessage == null ? null : new MessageReplyDto(message.ReplyToMessage.Id, message.ReplyToMessage.TextContent!, new MessageSenderDto(message.ReplyToMessage.Sender.Id, message.ReplyToMessage.Sender.Username, message.ReplyToMessage.Sender.FirstName, message.ReplyToMessage.Sender.LastName, message.ReplyToMessage.Sender.ProfilePhotoId)),
            new MessageSenderDto(message.Sender.Id, message.Sender.Username, message.Sender.FirstName, message.Sender.LastName, message.Sender.ProfilePhotoId),
            false);
}
