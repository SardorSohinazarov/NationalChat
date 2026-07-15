using Application.Abstractions.Persistence;
using Application.Features.Contacts.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Contacts;

public interface IContactRepository : IBaseRepository<Contact>
{
    Task<User?> FindUserAsync(string usernameOrEmail, CancellationToken cancellationToken);
    Task<bool> ContactExistsAsync(int userId, int contactUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ContactDto>> GetContactsAsync(int userId, CancellationToken cancellationToken);
    Task<Contact?> GetContactAsync(int userId, int contactId, CancellationToken cancellationToken);
}
