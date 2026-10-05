namespace Sakolee.Domain.Entities;

/// <summary>
/// One class a <see cref="Student"/> is enrolled in — a student may be in several. The first class picked
/// is also mirrored onto <see cref="Student.ClassId"/>, which older screens and queries still read; this
/// table is the full list. A student with no rows here (created before it existed) is enrolled in just
/// their <see cref="Student.ClassId"/>, if set.
/// <para>
/// Ids are native <c>uniqueidentifier</c> columns, unlike the legacy <c>Students</c> table's
/// <c>nvarchar(450)</c> ones, so no FK constraint is declared onto it (or onto <c>Class</c>).
/// </para>
/// </summary>
public class StudentClass : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid ClassId { get; set; }
}
