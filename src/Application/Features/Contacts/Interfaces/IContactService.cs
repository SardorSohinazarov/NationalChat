using Application.Features.Contacts.DataTransferObjects.Requests;
using Application.Features.Contacts.DataTransferObjects.Responses;

namespace Application.Features.Contacts;

public interface IContactService
{
    Task<IReadOnlyList<ContactDto>> GetContactsAsync(int userId, CancellationToken cancellationToken = default);
    Task<ContactDto?> AddContactAsync(int userId, AddContactRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteContactAsync(int userId, int contactId, CancellationToken cancellationToken = default);
}
