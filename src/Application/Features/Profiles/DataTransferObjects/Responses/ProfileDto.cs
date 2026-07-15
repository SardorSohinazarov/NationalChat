namespace Application.Features.Profiles.DataTransferObjects.Responses;

public sealed record ProfileDto(
    int Id,
    string Email,
    string Username,
    string FirstName,
    string? LastName,
    string? Bio,
    int? ProfilePhotoId,
    DateTime CreatedAt);
