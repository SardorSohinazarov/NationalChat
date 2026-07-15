namespace Application.DataTransferObjects.Pagination;

public sealed record CursorPagedResponse<T>(IReadOnlyList<T> Items, int? NextCursor, bool HasMore);
