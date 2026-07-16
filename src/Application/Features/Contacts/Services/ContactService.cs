using Application.DataTransferObjects.Pagination;
using Application.Features.Contacts.DataTransferObjects.Requests;
using Application.Features.Contacts.DataTransferObjects.Responses;
using Application.Features.Contacts.Factories;
using Application.Features.Contacts.Mappers;
using Domain.Entities;
using FluentValidation;

namespace Application.Features.Contacts;

public sealed class ContactService(IContactRepository repository, IValidator<AddContactRequest> addContactValidator) : IContactService
{
    public Task<CursorPagedResponse<ContactDto>> GetContactsAsync(
        int userId,
        CursorPaginationRequest pagination,
        CancellationToken cancellationToken = default) =>
        repository.GetContactsAsync(userId, pagination, cancellationToken);

    public async Task<ContactDto?> AddContactAsync(int userId, AddContactRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await addContactValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return null;
        }

        var identifier = request.UsernameOrEmail.Trim().ToLowerInvariant();
        var contactUser = await repository.FindUserAsync(identifier, cancellationToken);
        if (contactUser is null || contactUser.Id == userId || await repository.ContactExistsAsync(userId, contactUser.Id, cancellationToken))
        {
            return null;
        }

        var contact = ContactFactory.Create(userId, contactUser.Id, request.CustomFirstName, request.CustomLastName);
        await repository.AddAsync(contact, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        contact.ContactUser = contactUser;
        return ContactMapper.ToDto(contact);
    }

    public async Task<bool> DeleteContactAsync(int userId, int contactId, CancellationToken cancellationToken = default)
    {
        var contact = await repository.GetContactAsync(userId, contactId, cancellationToken);
        if (contact is null) return false;
        repository.Remove(contact);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
