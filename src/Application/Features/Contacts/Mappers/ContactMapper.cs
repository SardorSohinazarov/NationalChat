using System.Linq.Expressions;
using Application.Features.Contacts.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Contacts.Mappers;

public static class ContactMapper
{
    public static Expression<Func<Contact, ContactDto>> Projection => contact =>
        new(contact.Id, contact.ContactUserId, contact.ContactUser.Username, contact.ContactUser.FirstName,
            contact.ContactUser.LastName, contact.CustomFirstName, contact.CustomLastName);

    public static ContactDto ToDto(Contact contact) =>
        new(contact.Id, contact.ContactUserId, contact.ContactUser.Username, contact.ContactUser.FirstName,
            contact.ContactUser.LastName, contact.CustomFirstName, contact.CustomLastName);
}
