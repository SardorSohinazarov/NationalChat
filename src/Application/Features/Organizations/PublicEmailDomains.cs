namespace Application.Features.Organizations;

/// <summary>
/// Free mail services anyone can register at. They never identify an organization, so they are
/// rejected as organization domains and their users never join organization groups.
/// </summary>
public static class PublicEmailDomains
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        "gmail.com", "googlemail.com", "mail.ru", "yandex.ru", "yandex.com", "outlook.com",
        "hotmail.com", "yahoo.com", "icloud.com", "inbox.ru", "list.ru", "bk.ru"
    };
}
