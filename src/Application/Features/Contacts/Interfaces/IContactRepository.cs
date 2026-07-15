using Application.Abstractions.Persistence;
using Application.DataTransferObjects.Pagination;
using Application.Features.Contacts.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Contacts;

public interface IContactRepository : IBaseRepository<Contact>
{
    Task<User?> FindUserAsync(string usernameOrEmail, CancellationToken cancellationToken);
    Task<bool> ContactExistsAsync(int userId, int contactUserId, CancellationToken cancellationToken);
    Task<CursorPagedResponse<ContactDto>> GetContactsAsync(
        int userId,
        CursorPaginationRequest pagination,
        CancellationToken cancellationToken);
    Task<Contact?> GetContactAsync(int userId, int contactId, CancellationToken cancellationToken);
}
