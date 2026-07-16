using Application.Features.Contacts;
using Application.DataTransferObjects.Pagination;
using Application.Features.Contacts.DataTransferObjects.Responses;
using Application.Features.Contacts.Mappers;
using Domain.Entities;
using Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ContactRepository(ChatDb db) : BaseRepository<Contact>(db), IContactRepository
{
    public Task<User?> FindUserAsync(string usernameOrEmail, CancellationToken cancellationToken) =>
        Db.Users.FirstOrDefaultAsync(x => x.Username == usernameOrEmail || x.Email == usernameOrEmail, cancellationToken);

    public Task<bool> ContactExistsAsync(int userId, int contactUserId, CancellationToken cancellationToken) =>
        Db.Contacts.AnyAsync(x => x.UserId == userId && x.ContactUserId == contactUserId, cancellationToken);

    public Task<CursorPagedResponse<ContactDto>> GetContactsAsync(
        int userId,
        CursorPaginationRequest pagination,
        CancellationToken cancellationToken) =>
        Db.Contacts.AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToCursorPagedResponseAsync(
                pagination,
                x => x.Id,
                ContactMapper.Projection,
                x => x.Id,
                cancellationToken);

    public Task<Contact?> GetContactAsync(int userId, int contactId, CancellationToken cancellationToken) =>
        Db.Contacts.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == contactId, cancellationToken);
}
