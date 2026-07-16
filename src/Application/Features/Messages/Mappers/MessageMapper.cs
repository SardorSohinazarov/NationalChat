using System.Linq.Expressions;
using Application.Features.Messages.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Messages.Mappers;

public static class MessageMapper
{
    public static Expression<Func<Message, MessageDto>> Projection => message =>
        new(message.Id, message.ChatId, message.TextContent!, message.SentAt, message.ReplyToMessageId,
            new MessageSenderDto(message.Sender.Id, message.Sender.Username, message.Sender.FirstName, message.Sender.LastName, message.Sender.ProfilePhotoId));

    public static MessageDto ToDto(Message message) =>
        new(message.Id, message.ChatId, message.TextContent!, message.SentAt, message.ReplyToMessageId,
            new MessageSenderDto(message.Sender.Id, message.Sender.Username, message.Sender.FirstName, message.Sender.LastName, message.Sender.ProfilePhotoId));
}
