namespace Sakolee.Domain.Entities;

/// <summary>
/// A class offering (schedule, billing, enrollment rules, and online/portal settings). Not an
/// <see cref="AuditableEntity"/> — mirrors <see cref="Student"/>'s shape: the physical <c>Class</c>
/// table stores id-shaped columns as <c>nvarchar(450)</c> rather than <c>uniqueidentifier</c>, and audit
/// fields are stamped explicitly by <c>ClassesController</c> rather than a generic interceptor.
/// <para>
/// Carries no <c>TenantId</c> of its own — same as <see cref="Student"/> before it. Unlike Student
/// (scoped indirectly through <see cref="Student.PersonId"/>), Class has no linked entity to scope
/// through yet: <see cref="LocationId"/>/<see cref="RoomId"/>/<see cref="SessionId"/>/
/// <see cref="PrimaryInstructorId"/> reference features that do not exist in this app yet, so a class
/// row is currently visible platform-wide rather than confined to one tenant.
/// </para>
/// </summary>
public class Class
{
    public Guid Id { get; set; }

    /// <summary>Owning location (no Location management feature yet).</summary>
    public Guid? LocationId { get; set; }

    /// <summary>Owning room (no Room management feature yet).</summary>
    public Guid? RoomId { get; set; }

    /// <summary>Owning session/term (no Session management feature yet).</summary>
    public Guid? SessionId { get; set; }

    /// <summary>The class's primary instructor (no Instructor management feature yet).</summary>
    public Guid? PrimaryInstructorId { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public Guid? CreatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public Guid? UpdatedById { get; set; }

    public bool Active { get; set; } = true;

    public bool Deleted { get; set; }

    public string? ClassName { get; set; }

    /// <summary>Free text on the live schema, not a list of ids — preserved as-is.</summary>
    public string? AdditionalInstructors { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? RegistrationOpenDate { get; set; }

    /// <summary>Free text (e.g. a comma-separated day list) on the live schema — preserved as-is.</summary>
    public string? ActiveDays { get; set; }

    /// <summary>Free-text time of day (not a <see cref="TimeSpan"/>) on the live schema — preserved as-is.</summary>
    public string? StartTime { get; set; }

    /// <summary>Free-text time of day (not a <see cref="TimeSpan"/>) on the live schema — preserved as-is.</summary>
    public string? EndTime { get; set; }

    public string? Duration { get; set; }

    /// <summary>Maps to the physical "TutionFee" column (typo preserved from the live schema).</summary>
    public decimal? TuitionFee { get; set; }

    public string? BillingMethod { get; set; }

    public string? BillingCycle { get; set; }

    /// <summary>A flag on the live schema, not an amount — preserved as-is.</summary>
    public bool? RegistrationFee { get; set; }

    public string? Description { get; set; }

    /// <summary>Free text (not a fixed option) on the live schema — preserved as-is.</summary>
    public string? Gender { get; set; }

    public int? MinAge { get; set; }

    public int? MaxAge { get; set; }

    public int? MaxClassSize { get; set; }

    public int? MaxWaitlistSize { get; set; }

    public DateTime? CutoffDate { get; set; }

    /// <summary>Free text on the live schema, not a list of ids — preserved as-is.</summary>
    public string? PolicyGroups { get; set; }

    /// <summary>Maps to the physical "VirtualClassURL" column.</summary>
    public string? VirtualClassUrl { get; set; }

    public string? LinkDisplayText { get; set; }

    public bool OnlineListings { get; set; }

    public bool OnlineRegistration { get; set; }

    public bool AllowWaitlistInRoll { get; set; }

    public bool AllowPortalEnrollment { get; set; }

    public bool AllowDropIns { get; set; }

    public bool ParentPortalSchedule { get; set; }

    public bool MakeupsInClass { get; set; }

    public bool AllowWaitlistEnrollment { get; set; }

    public bool AllowPortalDropRequests { get; set; }

    /// <summary>A flag on the live schema, not an amount — preserved as-is.</summary>
    public bool DropInFee { get; set; }
}
