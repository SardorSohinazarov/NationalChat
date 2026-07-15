namespace Application.Features.Profiles.DataTransferObjects.Requests;

public sealed record UpdateProfileRequest(
    string Username,
    string FirstName,
    string? LastName,
    string? Bio);
