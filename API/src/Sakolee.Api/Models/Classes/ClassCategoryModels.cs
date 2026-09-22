namespace Sakolee.Api.Models.Classes;

/// <summary>A Class form Category 1/2/3 dropdown option.</summary>
public sealed record ClassCategorySummary(Guid ClassCategoryId, string Name, string? CategoryType,Guid TenantId,string TenantName,DateTime CreatedOnUtc);

/// <summary>
/// Request used to create a Class Category.
/// </summary>
public sealed record CreateClassCategoryRequest(string Name,string CategoryType);

/// <summary>
/// Request used to update a Class Category.
/// </summary>
public sealed record UpdateClassCategoryRequest(string Name,string CategoryType);