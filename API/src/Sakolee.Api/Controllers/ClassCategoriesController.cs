using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models.Classes;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;
using Sakolee.Application.Abstractions.Security;
namespace Sakolee.Api.Controllers;

/// <summary>
/// Provides administrative operations for Class Category records.
/// Class Categories are scoped to the caller's active tenant and are
/// used as options for Category 1, Category 2, and Category 3.
/// </summary>
[ApiController]
[Authorize]
[Route("/api/admin/class-categories")]
[Produces("application/json")]
[Tags("Classes")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class ClassCategoriesController : ControllerBase
{
    #region Fields
    // Repository used to perform database operations related to Class Category entities.
    private readonly IClassCategoryRepository _classCategories;
    // Unit of Work used to manage and save database changes.
    private readonly IUnitOfWork _unitOfWork;
    // Repository used to resolve Created By and Updated By user names.
    private readonly IUserRepository _users;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Class Categories controller.
    /// </summary>
    public ClassCategoriesController(IClassCategoryRepository classCategories,IUnitOfWork unitOfWork, IUserRepository users)
    {
        _classCategories = classCategories;
        _unitOfWork = unitOfWork;
        _users = users;
    }

    #endregion

    #region List 

    /// <summary>
    /// Gets all non-deleted Class Categories for the caller's active tenant. Also readable with
    /// classes.read: the Class form loads this list for its category dropdown.
    /// </summary>
    [HttpGet]
    [RequireAnyPermission(Permissions.ClassCategoriesRead, Permissions.ClassesRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? search = null,CancellationToken cancellationToken = default)
    {
        // Get the active tenant ID of the currently logged-in user.
        // If the user does not have an active tenant, return Forbidden.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Retrieve all non-deleted Class Categories for the active tenant.
        var categories = await _classCategories.ListByTenantAsync(tenantId,search,cancellationToken);
        // Resolve the Created By and Updated By user IDs into display names.
        var nameOf = await AuditNamesAsync(categories, cancellationToken);
        // Convert the entities into summary response models.
        var summaries = categories.Select(x => ToSummary(x, nameOf)).ToList();
        // Return the Class Category list in the standard API response format.
        return Ok(ApiResponseFactory.Success(summaries,"Class categories retrieved."));
    }
    #endregion


    #region Get
    /// <summary>
    /// Gets a Class Category by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.ClassCategoriesRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id,CancellationToken cancellationToken)
    {
        // Get the active tenant ID of the currently logged-in user. // If there is no active tenant, return Forbidden.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Get the category by ID for the active tenant.
        var category = await _classCategories.GetByIdAsync(id,tenantId,cancellationToken);
        if (category is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Class category not found."));
        }
        // Map the category entity to a summary response.
        var summary = ToSummary(category,await AuditNamesAsync( new[] { category },cancellationToken));
        // Return the Class Category details.
        return Ok(ApiResponseFactory.Success(summary,"Class category retrieved."));
    }

    #endregion

    #region Create

    /// <summary>
    /// Creates a new Class Category for the caller's active tenant.
    /// The category name must be unique within the tenant.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.ClassCategoriesWrite)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateClassCategoryRequest request,CancellationToken cancellationToken)
    {
        // Get the active tenant ID of the currently logged-in user. // If there is no active tenant, return Forbidden.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Validate that the Class Category name is provided.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new {  message = "Category name is required." });
        }
        // Validate that the Category Type is provided.
        if (string.IsNullOrWhiteSpace(request.CategoryType))
        {
            return BadRequest(new { message = "Category type is required." });
        }
        // Remove leading and trailing spaces from the request values.
        var name = request.Name.Trim();
        var categoryType = request.CategoryType.Trim();
        // Validate that the Category Type is one of the supported values.
        if (categoryType is not "Category 1" and not "Category 2" and not "Category 3")
        {
            return BadRequest(new { message = "Category type must be Category 1, Category 2, or Category 3." });
        }
        // Check whether a Class Category with the same name already exists for the active tenant.
        var nameExists = await _classCategories.ExistsByNameAsync(tenantId,name,cancellationToken: cancellationToken);
        // Return Conflict if a duplicate Class Category exists.
        if (nameExists)
        {
            return Conflict(new { message = $"A class category with the name '{name}' already exists."});
        }
        // Create the new Class Category entity.
        var classCategory = new Domain.Entities.ClassCategory { Id = Guid.NewGuid(), TenantId = tenantId, Name = name, CategoryType = categoryType,CreatedOnUtc = DateTime.UtcNow,CreatedById = User.GetUserId(), Deleted = false };
        // Add the new Class Category to the database context.
        await _classCategories.AddAsync(classCategory,cancellationToken);
        // Save the new Class Category to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Retrieve the created entity so that related information,such as the tenant, can be included in the response.
        var createdCategory = await _classCategories.GetByIdAsync( classCategory.Id,tenantId,cancellationToken);
        // Resolve the Created By and Updated By user names.
        var nameOf = await AuditNamesAsync(new[] { createdCategory ?? classCategory },cancellationToken);
        // Map the created entity to the summary response model.
        var summary = ToSummary(createdCategory ?? classCategory,nameOf);
        // Return the newly created Class Category.
        return StatusCode(StatusCodes.Status201Created,ApiResponseFactory.Success(summary,"Class category created."));
    }

    #endregion

    #region Update
    /// <summary>
    /// Updates an existing Class Category.
    /// The category name must be unique within the tenant,
    /// excluding the category currently being updated.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.ClassCategoriesWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id,[FromBody] UpdateClassCategoryRequest request,CancellationToken cancellationToken)
    {
        // Get the active tenant ID of the currently logged-in user.If there is no active tenant, return Forbidden.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Validate that the Class Category name is provided.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Category name is required."});
        }
        // Validate that the Category Type is provided.
        if (string.IsNullOrWhiteSpace(request.CategoryType))
        {
            return BadRequest(new { message = "Category type is required." });
        }
        // Remove leading and trailing spaces from the request values.
        var name = request.Name.Trim();
        var categoryType = request.CategoryType.Trim();
        // Validate that the Category Type is one of the supported values.
        if (categoryType is not "Category 1" and not "Category 2" and not "Category 3")
        {
            return BadRequest(new { message ="Category type must be Category 1, Category 2, or Category 3." });
        }
        // Get the existing Class Category for the active tenant.
        var classCategory = await _classCategories.GetByIdAsync(id,tenantId,cancellationToken);
        // Return Not Found if the Class Category does not exist.
        if (classCategory is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Class category not found."));
        }
        // Check whether another Class Category already uses the same name within the active tenant.
        var nameExists = await _classCategories.ExistsByNameAsync(tenantId,name, excludeId: id,cancellationToken: cancellationToken);
        // Return Conflict if another Class Category has the same name.
        if (nameExists)
        {
            return Conflict(new { message = $"A class category with the name '{name}' already exists." });
        }
        // Update the Class Category values.
        classCategory.Name = name;
        classCategory.CategoryType = categoryType;
        // Update audit information.
        classCategory.UpdatedOnUtc = DateTime.UtcNow;
        classCategory.UpdatedById = User.GetUserId();
        // Mark the Class Category as updated in the database context.
        _classCategories.Update(classCategory);
        // Save the changes to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Resolve the Created By and Updated By user names.
        var nameOf = await AuditNamesAsync(new[] { classCategory },cancellationToken);
        // Map the updated entity to the summary response model.
        var summary = ToSummary(classCategory,nameOf);
        // Return the updated Class Category.
        return Ok(ApiResponseFactory.Success(summary,"Class category updated."));
    }
    #endregion

    #region Delete

    /// <summary>
    /// Soft deletes a Class Category.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.ClassCategoriesDelete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id,CancellationToken cancellationToken)
    {
        // Get the active tenant ID of the currently logged-in user. If there is no active tenant, return Forbidden.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Get the Class Category by ID for the active tenant.
        var classCategory = await _classCategories.GetByIdAsync(id,tenantId,cancellationToken);
        // Return Not Found if the Class Category does not exist.
        if (classCategory is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Class category not found."));
        }
        // Mark the category as deleted instead of physically removing it.
        classCategory.Deleted = true;
        // Update the audit information.
        classCategory.UpdatedOnUtc = DateTime.UtcNow;
        classCategory.UpdatedById = User.GetUserId();
        // Mark the Class Category as updated in the database context.
        _classCategories.Update(classCategory);
        // Save the soft-delete changes to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Return the ID of the deleted Class Category.
        return Ok(ApiResponseFactory.Success(new { classCategoryId = classCategory.Id }, "Class category deleted."));
    }

    #endregion

    #region Summary

    /// <summary>
    /// Converts a Class Category entity into a summary response model.
    /// Resolves the Created By and Updated By user IDs into display names.
    /// </summary>
    private static ClassCategorySummary ToSummary(Domain.Entities.ClassCategory classCategory,Func<Guid?, string?> nameOf)
    {
        // Create and return the summary response using the Class Category entity values and resolved audit user names.
        return new ClassCategorySummary(classCategory.Id,classCategory.Name,classCategory.CategoryType,classCategory.TenantId,classCategory.Tenant?.Name ?? string.Empty,nameOf(classCategory.CreatedById), classCategory.CreatedOnUtc, nameOf(classCategory.UpdatedById),classCategory.UpdatedOnUtc);
    }

    #endregion

    #region Audit Names

    /// <summary>
    /// Resolves the Created By and Updated By user IDs into display names.
    /// </summary>
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<Domain.Entities.ClassCategory> rows,CancellationToken cancellationToken)
    {
        // Collect all Created By and Updated By user IDs from the records.
        var ids = rows.SelectMany(x => new[]{ x.CreatedById,x.UpdatedById }).Where(id => id.HasValue).Select(id => id!.Value).Distinct();
        // Retrieve the full names for the collected user IDs.
        var names = await _users.GetFullNamesAsync(ids,cancellationToken);
        // Return a function that resolves a user ID to its display name Return null when the user ID is not found.
        return id =>id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }

    #endregion
}