using Domain.Entities;

namespace Application.Features.Contacts.Factories;

public static class ContactFactory
{
    public static Contact Create(int userId, int contactUserId, string? customFirstName, string? customLastName) =>
        new()
        {
            UserId = userId,
            ContactUserId = contactUserId,
            CustomFirstName = NormalizeName(customFirstName),
            CustomLastName = NormalizeName(customLastName)
        };

    private static string? NormalizeName(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
