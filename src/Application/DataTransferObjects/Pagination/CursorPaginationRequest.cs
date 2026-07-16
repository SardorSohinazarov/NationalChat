namespace Application.DataTransferObjects.Pagination;

public sealed record CursorPaginationRequest
{
    public int Limit { get; init; } = 30;

    public int? BeforeId { get; init; }
}
