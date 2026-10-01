namespace Sakolee.Api.Models.StudentGradeLevels;

#region Create Student Grade Level Request
public sealed class CreateStudentGradeLevelRequest
{
    /// <summary>
    /// Gets or sets the name of the student grade level.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the student grade level is active.
    /// </summary>
    public bool Active { get; set; } = true;
}
#endregion

#region Update Student Grade Level Request
public sealed class UpdateStudentGradeLevelRequest
{
    /// <summary>
    /// Gets or sets the name of the student grade level.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the student grade level is active.
    /// </summary>
    public bool Active { get; set; }
}
#endregion

#region Student Grade Level Response
public sealed record StudentGradeLevelResponse(
    Guid Id,
    Guid? TenantId,
    string Name,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime UpdatedOnUtc);
#endregion

#region Student Grade Level Summary
public sealed record StudentGradeLevelSummary(
    Guid Id,
    Guid? TenantId,
    string Name,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime UpdatedOnUtc);
#endregion