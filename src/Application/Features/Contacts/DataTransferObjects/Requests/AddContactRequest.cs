namespace Application.Features.Contacts.DataTransferObjects.Requests;

public sealed record AddContactRequest(string UsernameOrEmail, string? CustomFirstName, string? CustomLastName);
