namespace Application.Features.Users.DataTransferObjects.Responses;

public sealed record UserSearchDto(
    int Id,
    string Username,
    string FirstName,
    string? LastName,
    int? ProfilePhotoId);
