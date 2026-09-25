namespace Domain.Entities;

/// <summary>
/// An organization is an e-mail domain: everything after the last "@" ("tuit.uz" and "student.tuit.uz" are two
/// organizations). It is created automatically when the first person with such an address signs in.
/// </summary>
public class Organization
{
    public int Id { get; set; }
    /// <summary>Lower-case organization domain without a trailing dot. Unique.</summary>
    public string Domain { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
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
