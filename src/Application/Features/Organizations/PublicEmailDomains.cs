namespace Application.Features.Organizations;

/// <summary>
/// Mail services anyone can register at. They never identify an organization, so their users get no
/// organization, badge or organization group.
/// </summary>
public static class PublicEmailDomains
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        // International
        "gmail.com", "googlemail.com", "outlook.com", "hotmail.com", "live.com", "msn.com", "yahoo.com",
        "ymail.com", "icloud.com", "me.com", "mac.com", "aol.com", "proton.me", "protonmail.com", "pm.me",
        "gmx.com", "gmx.net", "mail.com", "zoho.com", "yandex.com", "tutanota.com", "fastmail.com",
        // Russian-speaking region
        "mail.ru", "inbox.ru", "list.ru", "bk.ru", "internet.ru", "yandex.ru", "ya.ru", "rambler.ru",
        // Uzbekistan
        "umail.uz", "inbox.uz", "mail.uz",
        // Disposable / temporary
        "mailinator.com", "10minutemail.com", "temp-mail.org", "guerrillamail.com", "yopmail.com"
    };
}
