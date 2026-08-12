namespace Application.Features.Messages.DataTransferObjects.Requests;

public sealed record MessageSearchRequest(string Query, int Limit = 20);
