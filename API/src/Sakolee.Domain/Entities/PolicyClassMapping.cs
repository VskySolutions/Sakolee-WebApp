namespace Sakolee.Domain.Entities;

/// <summary>
/// One class a <see cref="Policy"/> applies to. A plain join row keyed on (PolicyId, ClassId) — not an
/// <see cref="AuditableEntity"/>, so removing a class from a policy deletes the row outright and the pair
/// can be re-added later. <c>Class</c> stores its ids as <c>nvarchar(450)</c>, so no FK is declared onto it.
/// </summary>
public class PolicyClassMapping
{
    public Guid PolicyId { get; set; }

    public Guid ClassId { get; set; }

    public Policy? Policy { get; set; }
}
