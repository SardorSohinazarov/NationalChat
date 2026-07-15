namespace Application.Features.Contacts.DataTransferObjects.Responses;

public sealed record ContactDto(int Id, int UserId, string Username, string FirstName, string? LastName, string? CustomFirstName, string? CustomLastName);
