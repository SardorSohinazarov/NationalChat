using Application.Features.Organizations;

namespace NationalChat.Tests.Organizations;

public sealed class OrganizationEmailMatcherTests
{
    [Theory]
    [InlineData("ali@tuit.uz", "tuit.uz")]
    [InlineData("Ali@TUIT.UZ.", "tuit.uz")]
    [InlineData("vali@student.tuit.uz", "student.tuit.uz")]
    [InlineData("hr@rtm.uz", "rtm.uz")]
    [InlineData("\"odd@name\"@tuit.uz", "tuit.uz")]
    [InlineData("ali@evil-tuit.uz", "evil-tuit.uz")]
    [InlineData("ali@tuit.uz.evil.com", "tuit.uz.evil.com")]
    public void OrganizationDomainOf_IsEverythingAfterLastAt(string email, string expected)
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
