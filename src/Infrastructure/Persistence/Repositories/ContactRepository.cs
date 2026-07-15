using Application.Features.Contacts;
using Application.Features.Contacts.DataTransferObjects.Responses;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ContactRepository(ChatDb db) : BaseRepository<Contact>(db), IContactRepository
{
    public Task<User?> FindUserAsync(string usernameOrEmail, CancellationToken cancellationToken) =>
        Db.Users.FirstOrDefaultAsync(x => x.Username == usernameOrEmail || x.Email == usernameOrEmail, cancellationToken);

    public Task<bool> ContactExistsAsync(int userId, int contactUserId, CancellationToken cancellationToken) =>
        Db.Contacts.AnyAsync(x => x.UserId == userId && x.ContactUserId == contactUserId, cancellationToken);

    public async Task<IReadOnlyList<ContactDto>> GetContactsAsync(int userId, CancellationToken cancellationToken) =>
        await Db.Contacts.Where(x => x.UserId == userId).OrderBy(x => x.ContactUser.Username)
            .Select(x => new ContactDto(x.Id, x.ContactUserId, x.ContactUser.Username, x.ContactUser.FirstName, x.ContactUser.LastName, x.CustomFirstName, x.CustomLastName))
            .ToListAsync(cancellationToken);

    public Task<Contact?> GetContactAsync(int userId, int contactId, CancellationToken cancellationToken) =>
        Db.Contacts.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == contactId, cancellationToken);
}
