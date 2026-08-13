using Application.Features.Files.DataTransferObjects.Requests;
using Application.Features.Files.DataTransferObjects.Responses;

namespace Application.Features.Files;

public interface IFileService
{
    Task<FileStoreResult> StoreImageAsync(StoreImageRequest request, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}

public sealed record FileStoreResult(StoredFile? File, string? Error);
