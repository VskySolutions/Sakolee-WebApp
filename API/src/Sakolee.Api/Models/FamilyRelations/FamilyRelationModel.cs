namespace Sakolee.Api.Models.FamilyRelations;

/// <summary>
/// Represents the payload required to create a new family relation lookup record.
/// </summary>
public sealed class CreateFamilyRelationRequest
{
    /// <summary>
    /// Gets or sets the name/title of the family relation (e.g., Mother, Father, Guardian).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the family relation option is active and available for selection.
    /// </summary>
    public bool Active { get; set; } = true;
}

/// <summary>
/// Represents the payload required to update an existing family relation lookup record.
/// </summary>
public sealed class UpdateFamilyRelationRequest
{
    /// <summary>
    /// Gets or sets the updated name/title of the family relation.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the family relation option is active.
    /// </summary>
    public bool Active { get; set; }
}

/// <summary>
/// Represents the detailed response data contract for a family relation entity.
/// </summary>
/// <param name="Id">The unique identifier of the family relation.</param>
/// <param name="TenantId">The unique identifier of the associated tenant, if applicable.</param>
/// <param name="Name">The name/title of the family relation.</param>
/// <param name="Active">Indicates whether the record is active.</param>
/// <param name="CreatedBy">The name of the user who created the record.</param>
/// <param name="CreatedOnUtc">The timestamp (in UTC) when the record was created.</param>
/// <param name="UpdatedBy">The name of the user who last updated the record.</param>
/// <param name="UpdatedOnUtc">The timestamp (in UTC) when the record was last updated.</param>
public sealed record FamilyRelationResponse(
    Guid Id,
    Guid? TenantId,
    string Name,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime? UpdatedOnUtc);

/// <summary>
/// Represents a lightweight summary data contract for a family relation entity, typically used for lists or dropdowns.
/// </summary>
/// <param name="Id">The unique identifier of the family relation.</param>
/// <param name="TenantId">The unique identifier of the associated tenant, if applicable.</param>
/// <param name="Name">The name/title of the family relation.</param>
/// <param name="Active">Indicates whether the record is active.</param>
/// <param name="CreatedBy">The name of the user who created the record.</param>
/// <param name="CreatedOnUtc">The timestamp (in UTC) when the record was created.</param>
/// <param name="UpdatedBy">The name of the user who last updated the record.</param>
/// <param name="UpdatedOnUtc">The timestamp (in UTC) when the record was last updated.</param>
public sealed record FamilyRelationSummary(
    Guid Id,
    Guid? TenantId,
    string Name,
    bool Active,
    string Tenant,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime? UpdatedOnUtc);