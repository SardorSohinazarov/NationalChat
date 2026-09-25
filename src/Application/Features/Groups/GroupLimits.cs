namespace Application.Features.Groups;

public static class GroupLimits
{
    public const int MaxMembers = 200;
    /// <summary>Organization groups (a whole faculty or company) may be much larger than ordinary ones.</summary>
    public const int MaxOrganizationMembers = 5000;
    public const int TitleMaxLength = 64;
    public const int DescriptionMaxLength = 255;
    public const int MaxMembersPerRequest = 100;

    public static int MaxMembersFor(bool isOrganizationGroup) => isOrganizationGroup ? MaxOrganizationMembers : MaxMembers;
}
