using System.Linq.Expressions;
using Application.Features.Messages.DataTransferObjects.Responses;
using Application.Features.Organizations.DataTransferObjects.Responses;
using Application.Features.Organizations.Mappers;
using Domain.Entities;

namespace Application.Features.Messages.Mappers;

public static class MessageMapper
{
    public static Expression<Func<Message, MessageDto>> Projection(int currentUserId, IQueryable<Photo> photos) => message =>
        new(message.Id, message.ChatId, message.TextContent!, message.SentAt, message.EditedAt, message.ReplyToMessageId,
            message.ReplyToMessage == null ? null : new MessageReplyDto(message.ReplyToMessage.Id, message.ReplyToMessage.TextContent!, new MessageSenderDto(message.ReplyToMessage.Sender.Id, message.ReplyToMessage.Sender.Username, message.ReplyToMessage.Sender.FirstName, message.ReplyToMessage.Sender.LastName, message.ReplyToMessage.Sender.ProfilePhotoId,
                message.ReplyToMessage.Sender.OrganizationMembership == null ? null : new OrganizationBadgeDto(message.ReplyToMessage.Sender.OrganizationMembership.Organization.Id, message.ReplyToMessage.Sender.OrganizationMembership.Organization.Domain))),
            new MessageSenderDto(message.Sender.Id, message.Sender.Username, message.Sender.FirstName, message.Sender.LastName, message.Sender.ProfilePhotoId,
                message.Sender.OrganizationMembership == null ? null : new OrganizationBadgeDto(message.Sender.OrganizationMembership.Organization.Id, message.Sender.OrganizationMembership.Organization.Domain)),
            message.SenderId == currentUserId && message.Views.Any(view => view.UserId != currentUserId),
            message.Attachments.Select(attachment => new MessageAttachmentDto(attachment.FileId, (int)attachment.Type, attachment.File.Name, attachment.File.MimeType, attachment.File.SizeBytes,
                attachment.Type == AttachmentType.Photo || attachment.Type == AttachmentType.Video ? photos.Where(photo => photo.FileId == attachment.FileId).Select(photo => photo.Width).FirstOrDefault() : 0,
                attachment.Type == AttachmentType.Photo || attachment.Type == AttachmentType.Video ? photos.Where(photo => photo.FileId == attachment.FileId).Select(photo => photo.Height).FirstOrDefault() : 0,
                ContentUrl(attachment.Type, attachment.FileId))).ToList(),
            message.ServiceAction);

    public static MessageDto ToDto(Message message) =>
        new(message.Id, message.ChatId, message.TextContent!, message.SentAt, message.EditedAt, message.ReplyToMessageId,
            message.ReplyToMessage == null ? null : new MessageReplyDto(message.ReplyToMessage.Id, message.ReplyToMessage.TextContent!, ToSenderDto(message.ReplyToMessage.Sender)),
            ToSenderDto(message.Sender),
            false,
            message.Attachments.Select(attachment => new MessageAttachmentDto(attachment.FileId, (int)attachment.Type, attachment.File.Name, attachment.File.MimeType, attachment.File.SizeBytes, 0, 0,
                ContentUrl(attachment.Type, attachment.FileId))).ToArray(),
            message.ServiceAction);

    private static MessageSenderDto ToSenderDto(User sender) =>
        new(sender.Id, sender.Username, sender.FirstName, sender.LastName, sender.ProfilePhotoId, OrganizationMapper.ToBadge(sender));

    private static string ContentUrl(AttachmentType type, int fileId) => type switch
    {
        AttachmentType.File => $"/api/media/files/{fileId}",
        AttachmentType.Video => $"/api/media/videos/{fileId}",
        _ => $"/api/media/images/{fileId}",
    };
}
