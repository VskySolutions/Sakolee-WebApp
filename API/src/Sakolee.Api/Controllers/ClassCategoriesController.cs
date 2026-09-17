using Sakolee.Api.Models.Classes;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sakolee.Api.Controllers;

/// <summary>Read-only lookup for the Class form's Category 1/2/3 dropdown options, scoped to the caller's active tenant.</summary>
[ApiController]
[Authorize]
[Route("/api/admin/class-categories")]
[Produces("application/json")]
[Tags("Classes")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class ClassCategoriesController : ControllerBase
{
    private readonly IClassCategoryRepository _classCategories;

    public ClassCategoriesController(IClassCategoryRepository classCategories) => _classCategories = classCategories;

    [HttpGet]
    [RequirePermission(Permissions.ClassesRead)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }

        var categories = await _classCategories.ListByTenantAsync(tenantId, cancellationToken);
        var summaries = categories.Select(c => new ClassCategorySummary(c.Id, c.Name, c.CategoryType));
        return Ok(ApiResponseFactory.Success(summaries, "Class categories retrieved."));
    }
}
