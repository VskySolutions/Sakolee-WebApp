namespace Sakolee.Domain.Entities;

/// <summary>
/// A selectable option for one of a <see cref="Class"/>'s three category dropdowns (Category 1/2/3),
/// which one told apart by <see cref="CategoryType"/>. Scoped to the owning tenant. Not an
/// <see cref="AuditableEntity"/> — mirrors <see cref="Class"/>'s shape: audit fields are nullable and
/// stamped explicitly by <c>ClassCategoriesController</c> rather than the generic interceptor, and
/// there is no <c>DeletedOnUtc</c> column on the live schema. Id-shaped columns are stored as
/// <c>nvarchar(450)</c> rather than <c>uniqueidentifier</c> on the live schema, the same convention
/// <c>Class</c>/<c>Student</c> use.
/// </summary>
public class ClassCategory 
{
    #region Properties
    public Guid Id { get; set; }

    /// <summary>Owning tenant — maps to the physical "TenentId" column (typo preserved from the live schema).</summary>
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Which dropdown this option belongs to on the Class form: "Category 1", "Category 2", or "Category 3".</summary>
    public string? CategoryType { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public Guid? CreatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public Guid? UpdatedById { get; set; }

    public bool Deleted { get; set; }

    /// <summary>
    /// Navigation property for the owning tenant.
    /// </summary>
    public Tenant? Tenant { get; set; }
    #endregion
}
