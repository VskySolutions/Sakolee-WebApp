namespace Sakolee.Api.Models.ClassRooms;

/// <summary>
/// Represents the payload required to create a new class room record.
/// </summary>

public sealed class CreateClassRoomRequest
{
    public Guid LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}

/// <summary>
/// Represents the payload required to update an existing class room record.
/// </summary>
public sealed class UpdateClassRoomRequest
{

    public Guid LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }


}

/// <summary>
/// Represents the detailed response data contract for a class room entity.
/// </summary>
public sealed record ClassRoomResponse(
    Guid Id,
    Guid? TenantId,
    Guid LocationId,
    string Name,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime? UpdatedOnUtc);

public sealed record ClassRoomSummary(
    Guid Id,
    Guid? TenantId,
    Guid LocationId,
    string Name,
    bool Active,
    string Tenant,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime? UpdatedOnUtc);