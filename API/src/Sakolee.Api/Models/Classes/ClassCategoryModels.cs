namespace Sakolee.Api.Models.Classes;

/// <summary>A Class form Category 1/2/3 dropdown option.</summary>
public sealed record ClassCategorySummary(Guid ClassCategoryId, string Name, string? CategoryType);
