namespace Application.Features.Files;

public interface IAntivirusScanner
{
    Task<bool> IsCleanAsync(byte[] content, CancellationToken cancellationToken = default);
}
