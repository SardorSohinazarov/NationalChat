namespace Application.Features.Media;

public interface IAntivirusScanner
{
    Task<bool> IsCleanAsync(byte[] content, CancellationToken cancellationToken = default);
}
