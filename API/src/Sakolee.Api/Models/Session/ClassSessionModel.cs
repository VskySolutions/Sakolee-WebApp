namespace Sakolee.Api.Models.Session
{
    /// <summary>
    /// Create payload for a standalone Session master record.
    /// </summary>
    public sealed class CreateSessionRequest
    {
        /// <summary>
        /// The unique name of the session.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Flag indicating whether the session is active upon creation.
        /// </summary>
        public bool? IsActive { get; set; } = true; // Enabled to handle active/inactive status during creation

        public bool IsDeleted { get; set; } = false;
       
    }

    /// <summary>
    /// Update payload for an existing session record.
    /// </summary>
    public sealed class UpdateSessionRequest
    {
        /// <summary>
        /// The updated name of the session.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Flag indicating whether the session is active.
        /// </summary>
        public bool? IsActive { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// List-row projection for the Session grid with audit columns and tenant details.
    /// </summary>
    public sealed record SessionSummary(
        Guid Id,
        string Name,
        bool Active,
        string? CreatedBy,
        string? UpdatedBy,
        DateTime CreatedOn,
        DateTime? UpdatedOn,
        Guid TenantId,
        string Tenant,
        // Soft-deleted rows only appear with showDeleted; the Sessions page strikes them through.
        bool IsDeleted = false
    );

    /// <summary>
    /// Lightweight option for dropdown selections.
    /// </summary>
    public sealed record SessionSelectItem(
        Guid Id,
        string Name,
        string? DanceStyle,
        bool Active
    );

    /// <summary>
    /// Full detailed response record for a session entry with audit tracking.
    /// </summary>
    public sealed record SessionDetail(
        Guid Id,
        string Name,
        bool IsActive,
        bool IsDeleted,
        
        string? CreatedBy,
        string? UpdatedBy,
        DateTime CreatedOn,
        DateTime? UpdatedOn,
        DateTime? DeletedOnUtc,
        Guid TenantId,
        string? TenantName
    );
}