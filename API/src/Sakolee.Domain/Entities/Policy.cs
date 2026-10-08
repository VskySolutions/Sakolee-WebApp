namespace Sakolee.Domain.Entities;

/// <summary>
/// A tenant policy (refund, attendance, dress code, ...) — a named block of rich-text content that can be
/// attached to one or more classes through <see cref="PolicyClassMapping"/>.
/// </summary>
public class Policy : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Short plain-text summary shown in lists.</summary>
    public string? Description { get; set; }

    /// <summary>The policy text itself, stored as HTML from the rich-text editor.</summary>
    public string? Content { get; set; }

    public bool Active { get; set; } = true;

    public int DisplayOrder { get; set; }

    public ICollection<PolicyClassMapping> ClassMappings { get; set; } = new List<PolicyClassMapping>();
}
