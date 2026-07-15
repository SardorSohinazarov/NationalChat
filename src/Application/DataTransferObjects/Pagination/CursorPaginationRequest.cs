using System.ComponentModel.DataAnnotations;

namespace Application.DataTransferObjects.Pagination;

public sealed record CursorPaginationRequest
{
    [Range(1, 100)]
    public int Limit { get; init; } = 30;

    [Range(1, int.MaxValue)]
    public int? BeforeId { get; init; }
}
