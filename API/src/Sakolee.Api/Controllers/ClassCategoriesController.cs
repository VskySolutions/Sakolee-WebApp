using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models.Classes;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;

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

    private readonly IClassCategoryRepository _classCategories;
    private readonly IUnitOfWork _unitOfWork;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Class Categories controller.
    /// </summary>
    public ClassCategoriesController(
        IClassCategoryRepository classCategories,
        IUnitOfWork unitOfWork)
    {
        _classCategories = classCategories;
        _unitOfWork = unitOfWork;
    }

    #endregion

    #region Get

    /// <summary>
    /// Gets all non-deleted Class Categories for the caller's active tenant.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.ClassesRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseFactory.Forbidden(
                    "No active tenant for the caller."));
        }

        var categories = await _classCategories.ListByTenantAsync(
            tenantId,
            search,
            cancellationToken);

        var summaries = categories.Select(
            c => new ClassCategorySummary(
                c.Id,
                c.Name,
                c.CategoryType,
                c.TenantId,
                c.Tenant?.Name ?? string.Empty,
                c.CreatedOnUtc));

        return Ok(
            ApiResponseFactory.Success(
                summaries,
                "Class categories retrieved."));
    }

    /// <summary>
    /// Gets a Class Category by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.ClassesRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseFactory.Forbidden(
                    "No active tenant for the caller."));
        }

        var category = await _classCategories.GetByIdAsync(
            id,
            tenantId,
            cancellationToken);

        if (category is null)
        {
            return NotFound(
                ApiResponseFactory.NotFound(
                    "Class category not found."));
        }

        var summary = new ClassCategorySummary(
            category.Id,
            category.Name,
            category.CategoryType,
            category.TenantId,
            category.Tenant?.Name ?? string.Empty,
            category.CreatedOnUtc);

        return Ok(
            ApiResponseFactory.Success(
                summary,
                "Class category retrieved."));
    }

    #endregion

    #region Create

    /// <summary>
    /// Creates a new Class Category for the caller's active tenant.
    /// The category name must be unique within the tenant.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.ClassesWrite)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateClassCategoryRequest request,
        CancellationToken cancellationToken)
    {
        #region Tenant Validation

        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }

        #endregion

        #region Request Validation

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Category name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.CategoryType))
        {
            return BadRequest(new
            {
                message = "Category type is required."
            });
        }

        var name = request.Name.Trim();
        var categoryType = request.CategoryType.Trim();

        if (categoryType is not "Category 1"
            and not "Category 2"
            and not "Category 3")
        {
            return BadRequest(new
            {
                message =
                    "Category type must be Category 1, Category 2, or Category 3."
            });
        }

        #endregion

        #region Duplicate Validation

        var nameExists = await _classCategories.ExistsByNameAsync(tenantId,name,cancellationToken: cancellationToken);
        if (nameExists)
        {
            return Conflict(new
            {
                message = $"A class category with the name '{name}' already exists."
            });
        }

        #endregion

        #region Create Entity

        var classCategory = new Domain.Entities.ClassCategory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            CategoryType = categoryType,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedById = User.GetUserId(),
            Deleted = false
        };

        #endregion

        #region Save

        await _classCategories.AddAsync(
            classCategory,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        #endregion

        #region Response

        var createdCategory = await _classCategories.GetByIdAsync(
            classCategory.Id,
            tenantId,
            cancellationToken);

        var summary = new ClassCategorySummary(
            classCategory.Id,
            classCategory.Name,
            classCategory.CategoryType,
            classCategory.TenantId,
            createdCategory?.Tenant?.Name ?? string.Empty,
            classCategory.CreatedOnUtc);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponseFactory.Success(
                summary,
                "Class category created."));

        #endregion
    }

    #endregion

    #region Update

    /// <summary>
    /// Updates an existing Class Category.
    /// The category name must be unique within the tenant,
    /// excluding the category currently being updated.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.ClassesWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id,[FromBody] UpdateClassCategoryRequest request,CancellationToken cancellationToken)
    {
        #region Tenant Validation

        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }

        #endregion

        #region Request Validation

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Category name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.CategoryType))
        {
            return BadRequest(new
            {
                message = "Category type is required."
            });
        }

        var name = request.Name.Trim();
        var categoryType = request.CategoryType.Trim();

        if (categoryType is not "Category 1"
            and not "Category 2"
            and not "Category 3")
        {
            return BadRequest(new
            {
                message ="Category type must be Category 1, Category 2, or Category 3."
            });
        }

        #endregion

        #region Existing Category Validation

        var classCategory = await _classCategories.GetByIdAsync(id,tenantId,cancellationToken);
        if (classCategory is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Class category not found."));
        }

        #endregion

        #region Duplicate Validation

        var nameExists = await _classCategories.ExistsByNameAsync(tenantId,name, excludeId: id,cancellationToken: cancellationToken);

        if (nameExists)
        {
            return Conflict(new
            {
                message = $"A class category with the name '{name}' already exists."
            });
        }

        #endregion

        #region Update Entity

        classCategory.Name = name;
        classCategory.CategoryType = categoryType;
        classCategory.UpdatedOnUtc = DateTime.UtcNow;
        classCategory.UpdatedById = User.GetUserId();

        #endregion

        #region Save

        _classCategories.Update(classCategory);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        #endregion

        #region Response

        var summary = new ClassCategorySummary(
            classCategory.Id,
            classCategory.Name,
            classCategory.CategoryType,
            classCategory.TenantId,
            classCategory.Tenant?.Name ?? string.Empty,
            classCategory.CreatedOnUtc);

        return Ok(
            ApiResponseFactory.Success(
                summary,
                "Class category updated."));

        #endregion
    }

    #endregion

    #region Delete

    /// <summary>
    /// Soft deletes a Class Category.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.ClassesDelete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseFactory.Forbidden(
                    "No active tenant for the caller."));
        }

        var classCategory = await _classCategories.GetByIdAsync(
            id,
            tenantId,
            cancellationToken);

        if (classCategory is null)
        {
            return NotFound(
                ApiResponseFactory.NotFound(
                    "Class category not found."));
        }

        classCategory.Deleted = true;
        classCategory.UpdatedOnUtc = DateTime.UtcNow;
        classCategory.UpdatedById = User.GetUserId();

        _classCategories.Update(classCategory);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Ok(
            ApiResponseFactory.Success(
                new
                {
                    classCategoryId = classCategory.Id
                },
                "Class category deleted."));
    }

    #endregion
}