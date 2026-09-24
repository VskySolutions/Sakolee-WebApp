namespace Sakolee.Domain.Entities;

/// <summary>
/// A contact on a <see cref="Entities.Family"/>. Maps onto the physical <c>FamilyPersonMapping</c> table
/// (renamed from <c>FamilyContacts</c>, itself renamed from the legacy <c>ParentContacts</c> — see
/// <see cref="Entities.Family"/>'s remarks) — one row per contact, primary and secondary alike, keyed by
/// <see cref="IsPrimaryContact"/>.
/// <para>
/// The primary contact's identity is ALSO inlined directly on <see cref="Entities.Family"/> itself (that
/// row's own <see cref="Entities.Family.PersonId"/>/name/email/phone — see its remarks, a holdover from
/// the legacy <c>Parents</c> shape); its <see cref="FamilyPersonMapping"/> row here is a second,
/// authoritative copy kept in sync by <c>FamiliesController</c> so every contact — primary or secondary
/// — is reachable the same way, as one row per family under a shared <see cref="FamilyId"/>.
/// </para>
/// <para>
/// <see cref="PersonId"/>, <see cref="Relation"/>, <see cref="IsBillingContact"/>,
/// <see cref="IsAuthorizedToPickUpStudent"/>, and <see cref="IsPrimaryContact"/> did not exist on the
/// legacy table — added by the <c>ExtendParentsAndParentContactsForFamilies</c> and
/// <c>AddIsPrimaryContactToFamilyPersonMapping</c> migrations so every contact can get its own CRM
/// Person (and login account) and be told apart as primary or secondary.
/// </para>
/// </summary>
public class FamilyPersonMapping
{
    public Guid Id { get; set; }

    /// <summary>FK onto <see cref="Entities.Family"/>.<see cref="Entities.Family.Id"/>. Originally named
    /// <c>ParentId</c> (from before this table was understood as belonging to a Family) — renamed to
    /// <c>FamilyId</c> by <c>RenameFamilyPersonMappingParentIdToFamilyId</c>.</summary>
    public Guid FamilyId { get; set; }

    /// <summary>Unresolved legacy FK placeholder (shaped like every other id-as-string column on this
    /// table) — there has never been a ContactType table to point at. Preserved as-is, unused.</summary>
    public Guid? ContactTypeId { get; set; }

    /// <summary>Combined display name (not split first/last) on the legacy schema. Kept in sync with the
    /// linked Person's name once <see cref="PersonId"/> is set; treat the Person as authoritative.</summary>
    public string? ContactName { get; set; }

    public string? EmailAddress { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public Guid? CreatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public Guid? UpdatedById { get; set; }

    public bool Active { get; set; } = true;

    public bool Deleted { get; set; }

    // ---- Added by ExtendParentsAndParentContactsForFamilies — see class remarks ----

    /// <summary>This contact's linked CRM Person — minted alongside the contact, the same way
    /// <see cref="Entities.Family.PersonId"/> is minted for the primary contact.</summary>
    public Guid? PersonId { get; set; }

    /// <summary>Free-text relationship to the student(s), e.g. "Mother", "Father", "Guardian" — mirrors
    /// <see cref="Entities.Family.Type"/> for the primary contact.</summary>
    public string? Relation { get; set; }

    public bool IsBillingContact { get; set; }

    public bool IsAuthorizedToPickUpStudent { get; set; }

    /// <summary>Whether this row is the family's primary contact (also inlined on
    /// <see cref="Entities.Family"/> itself — see class remarks) or a secondary one.</summary>
    public bool IsPrimaryContact { get; set; }

    public Family? Family { get; set; }
}
