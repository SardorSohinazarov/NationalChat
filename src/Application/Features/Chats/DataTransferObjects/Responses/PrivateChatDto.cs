namespace Application.Features.Chats.DataTransferObjects.Responses;

public sealed record PrivateChatParticipantDto(int Id, string Username, string FirstName, string? LastName, int? ProfilePhotoId, bool IsOnline, DateTime? LastSeenAt);

public sealed record PrivateChatDto(int Id, DateTime CreatedAt, PrivateChatParticipantDto Participant);
