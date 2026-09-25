namespace Application.Features.Users.DataTransferObjects.Requests;

public sealed record UserSearchRequest(string Query, int Limit = 20);
