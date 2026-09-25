using Application.Features.Organizations;

namespace NationalChat.Tests.Organizations;

public sealed class OrganizationEmailMatcherTests
{
    [Theory]
    [InlineData("ali@tuit.uz", "tuit.uz")]
    [InlineData("Ali@TUIT.UZ.", "tuit.uz")]
    [InlineData("vali@student.tuit.uz", "tuit.uz")]
    [InlineData("hr@rtm.uz", "rtm.uz")]
    [InlineData("dekan@urdu.edu.uz", "urdu.edu.uz")]
    [InlineData("a@mail.company.co.uk", "company.co.uk")]
    [InlineData("ali@evil-tuit.uz", "evil-tuit.uz")]
    [InlineData("ali@tuit.uz.evil.com", "evil.com")]
    public void OrganizationDomainOf_UsesRegistrableDomain(string email, string expected)
    {
        Assert.Equal(expected, OrganizationEmailMatcher.OrganizationDomainOf(email));
    }

    [Theory]
    [InlineData("ali@gmail.com")]
    [InlineData("ali@GMAIL.com")]
    [InlineData("ali@mail.ru")]
    [InlineData("ali@yandex.ru")]
    [InlineData("ali@umail.uz")]
    [InlineData("ali@mailinator.com")]
    [InlineData("not-an-email")]
    [InlineData("ali@localhost")]
    [InlineData("")]
    public void OrganizationDomainOf_PublicOrInvalid_IsNull(string email)
    {
        Assert.Null(OrganizationEmailMatcher.OrganizationDomainOf(email));
    }

    [Theory]
    [InlineData("tuit.uz", "TUIT")]
    [InlineData("rtm.uz", "RTM")]
    [InlineData("urdu.edu.uz", "URDU")]
    public void ShortNameFor_IsFirstLabelInUpperCase(string domain, string expected)
    {
        Assert.Equal(expected, OrganizationEmailMatcher.ShortNameFor(domain));
    }

    [Theory]
    [InlineData("  Tuit.UZ. ", "tuit.uz")]
    [InlineData("localhost", null)]
    [InlineData("tuit..uz", null)]
    [InlineData("-tuit.uz", null)]
    [InlineData("tu it.uz", null)]
    [InlineData("tuit.uz/x", null)]
    public void NormalizeDomain(string domain, string? expected)
    {
        Assert.Equal(expected, OrganizationEmailMatcher.NormalizeDomain(domain));
    }
}
