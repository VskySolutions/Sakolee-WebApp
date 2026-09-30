namespace Sakolee.Api.Models.FamilyStatus
{
    /// <summary>
    /// Create payload for a standalone FamilyStatus master record.
    /// </summary>
    public sealed class CreateFamilyStatusRequest
    {
        /// <summary>
        /// The unique name of the family status.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        public Guid TenantId { get; set; }

        /// <summary>
        /// Whether the family status is active (offered in the Family form's status dropdown).
        /// </summary>
        public bool Active { get; set; } = true;
    }

    /// <summary>
    /// Update payload for an existing family status record.
    /// </summary>
    public sealed class UpdateFamilyStatusRequest
    {
        /// <summary>
        /// The updated name of the family status.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Flag indicating whether the family status is active.
        /// </summary>
        public bool? Active { get; set; }
    }

    /// <summary>
    /// List-row projection for the Family Status grid with audit columns.
    /// </summary>
    public sealed record FamilyStatusSummary(
        Guid Id,
        string Name,
        bool Active,
        string? CreatedBy,
        string? UpdatedBy,
        DateTime CreatedOnUtc,
        DateTime UpdatedOnUtc,
        Guid TenantId,
        string? Tenant
    );

    /// <summary>
    /// Lightweight option for dropdown selections.
    /// </summary>
    public sealed record FamilyStatusSelectItem(
        Guid Id,
        string Name,
        bool Active
    );

    /// <summary>
    /// Full detailed response record for a family status entry with audit tracking.
    /// </summary>
    public sealed record FamilyStatusDetail(
        Guid Id,
        string Name,
        bool Active,
        string? CreatedBy,
        string? UpdatedBy,
        DateTime CreatedOnUtc,
        DateTime UpdatedOnUtc
    );
}
