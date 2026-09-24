namespace Sakolee.Api.Models.Classes;

public sealed class CreateClassRequest
{
    public Guid? LocationId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? PrimaryInstructorId { get; set; }
    public Guid? Category1Id { get; set; }
    public Guid? Category2Id { get; set; }
    public Guid? Category3Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? AdditionalInstructors { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? RegistrationOpenDate { get; set; }
    public string? ActiveDays { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Duration { get; set; }
    public decimal? TuitionFee { get; set; }
    public string? BillingMethod { get; set; }
    public string? BillingCycle { get; set; }
    public bool? RegistrationFee { get; set; }
    public string? Description { get; set; }
    public string? Gender { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public int? MaxClassSize { get; set; }
    public int? MaxWaitlistSize { get; set; }
    public DateTime? CutoffDate { get; set; }
    public string? PolicyGroups { get; set; }
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
    public bool DropInFee { get; set; }
}

public sealed class UpdateClassRequest
{
    public Guid? LocationId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? PrimaryInstructorId { get; set; }
    public Guid? Category1Id { get; set; }
    public Guid? Category2Id { get; set; }
    public Guid? Category3Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? AdditionalInstructors { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? RegistrationOpenDate { get; set; }
    public string? ActiveDays { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Duration { get; set; }
    public decimal? TuitionFee { get; set; }
    public string? BillingMethod { get; set; }
    public string? BillingCycle { get; set; }
    public bool? RegistrationFee { get; set; }
    public string? Description { get; set; }
    public string? Gender { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public int? MaxClassSize { get; set; }
    public int? MaxWaitlistSize { get; set; }
    public DateTime? CutoffDate { get; set; }
    public string? PolicyGroups { get; set; }
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
    public bool DropInFee { get; set; }
    public bool Active { get; set; } = true;
}

/// <summary>A class row, full detail.</summary>
public sealed record ClassSummary(
    Guid ClassId,
    Guid? LocationId,
    Guid? RoomId,
    Guid? SessionId,
    Guid? PrimaryInstructorId,
    Guid? Category1Id,
    Guid? Category2Id,
    Guid? Category3Id,
    string? ClassName,
    string? AdditionalInstructors,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? RegistrationOpenDate,
    string? ActiveDays,
    string? StartTime,
    string? EndTime,
    string? Duration,
    decimal? TuitionFee,
    string? BillingMethod,
    string? BillingCycle,
    bool? RegistrationFee,
    string? Description,
    string? Gender,
    int? MinAge,
    int? MaxAge,
    int? MaxClassSize,
    int? MaxWaitlistSize,
    DateTime? CutoffDate,
    string? PolicyGroups,
    string? VirtualClassUrl,
    string? LinkDisplayText,
    bool OnlineListings,
    bool OnlineRegistration,
    bool AllowWaitlistInRoll,
    bool AllowPortalEnrollment,
    bool AllowDropIns,
    bool ParentPortalSchedule,
    bool MakeupsInClass,
    bool AllowWaitlistEnrollment,
    bool AllowPortalDropRequests,
    bool DropInFee,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime? UpdatedOnUtc,
    // Names of the saved Category 1/2/3 — filled on the single-class read only (the view/edit form
    // shows them even when the category dropdown's own list can't be loaded).
    string? Category1Name = null,
    string? Category2Name = null,
    string? Category3Name = null);
