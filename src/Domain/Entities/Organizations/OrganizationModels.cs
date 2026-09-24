namespace Domain.Entities;

/// <summary>An organization (university, company) whose members are verified by their e-mail domain.</summary>
public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<OrganizationDomain> Domains { get; set; } = new List<OrganizationDomain>();
    public ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
}

public class OrganizationDomain
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    /// <summary>Lower-case domain without a trailing dot, e.g. "tuit.uz". Unique across organizations.</summary>
    public string Domain { get; set; } = string.Empty;

    public Organization Organization { get; set; }
}

/// <summary>Verified membership. A user belongs to at most one organization (one e-mail, one domain).</summary>
public class OrganizationMember
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int UserId { get; set; }
    public OrganizationRole Role { get; set; }
    public DateTime VerifiedAt { get; set; }

    public Organization Organization { get; set; }
    public User User { get; set; }
}
