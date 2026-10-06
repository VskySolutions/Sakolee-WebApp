namespace Sakolee.Domain.Entities;

/// <summary>
/// Represents a Hear About Us master record.
/// Each record belongs to a specific tenant.
/// </summary>
public class HearAboutUs : AuditableEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the unique identifier of the Hear About Us record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// Maps to the physical "TenentId" column.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the name of the Hear About Us option.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the Hear About Us option is active.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>Stable key of a seeded default row (null for rows a tenant adds itself).</summary>
    public string? Code { get; set; }

    /// <summary>Template rows (in the template tenant) with this set are copied into every new tenant.</summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Navigation property for the owning tenant.
    /// </summary>
    public Tenant? Tenant { get; set; }

    #endregion
}