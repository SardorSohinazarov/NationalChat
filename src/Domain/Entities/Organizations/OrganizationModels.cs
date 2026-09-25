namespace Domain.Entities;

/// <summary>
/// An organization is identified by its e-mail domain (e.g. "tuit.uz"). It is created automatically when the
/// first person with such an address signs in; nobody registers it by hand.
/// </summary>
public class Organization
{
    public int Id { get; set; }
    /// <summary>Lower-case organization domain without a trailing dot. Unique.</summary>
    public string Domain { get; set; } = string.Empty;
    /// <summary>Badge text derived from the domain, e.g. "TUIT" for tuit.uz.</summary>
    public string ShortName { get; set; } = string.Empty;
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
