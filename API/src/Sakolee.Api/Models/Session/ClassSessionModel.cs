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
        /// The dance style associated with the session.
        /// </summary>
        public string? DanceStyle { get; set; }

        /// <summary>
        /// The schedule or timing of the session.
        /// </summary>
        public string? Timing { get; set; }

        //public Guid TenantId { get; set; }
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
        /// The updated dance style of the session.
        /// </summary>
        public string? DanceStyle { get; set; }

        /// <summary>
        /// The updated timing of the session.
        /// </summary>
        public string? Timing { get; set; }

        /// <summary>
        /// Flag indicating whether the session is active.
        /// </summary>
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// List-row projection for the Session grid with audit columns and tenant details.
    /// </summary>
    public sealed record SessionSummary(
        Guid Id,
        string Name,
        bool IsActive,
        string? CreatedBy,
        string? UpdatedBy,
        DateTime CreatedOn,
        DateTime? UpdatedOn,
        Guid TenantId,                    // Added TenantId
        string Tenant                     // Added Tenant Name
    );

    /// <summary>
    /// Lightweight option for dropdown selections.
    /// </summary>
    public sealed record SessionSelectItem(
        Guid Id,
        string Name,
        string? DanceStyle,
        bool IsActive
    );

    /// <summary>
    /// Full detailed response record for a session entry with audit tracking.
    /// </summary>
    public sealed record SessionDetail(
        Guid Id,
        string Name,
        bool IsActive,
        string? CreatedBy,
        string? UpdatedBy,
        DateTime CreatedOn,
        DateTime? UpdatedOn,
        Guid TenantId,
        string? TenantName
    );
}