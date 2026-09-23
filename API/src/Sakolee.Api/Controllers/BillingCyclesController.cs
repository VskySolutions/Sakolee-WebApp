using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models.BillingCycles;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Provides administrative operations for Billing Cycle records.
/// Billing Cycles are scoped to the caller's active tenant.
/// </summary>
[ApiController]
[Authorize]
[Route("/api/admin/billing-cycles")]
[Produces("application/json")]
[Tags("Billing Cycles")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class BillingCyclesController : ControllerBase
{
    #region Fields
    // Repository used to perform Billing Cycle database operations.
    private readonly IBillingCycleRepository _billingCycles;
    // Unit of Work used to save database changes.
    private readonly IUnitOfWork _unitOfWork;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Billing Cycles controller.
    /// </summary>
    public BillingCyclesController(IBillingCycleRepository billingCycles,IUnitOfWork unitOfWork)
    {
        _billingCycles = billingCycles;
        _unitOfWork = unitOfWork;
    }

    #endregion

    #region List 

    /// <summary>
    /// Gets all non-deleted Billing Cycles for the caller's active tenant.
    /// Supports searching by billing cycle name.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.BillingCyclesRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List( [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden( "No active tenant for the caller."));
        }
        var billingCycles = await _billingCycles.ListByTenantAsync(tenantId,search,cancellationToken);
        var summaries = billingCycles.Select(x => new BillingCycleSummary(x.Id, x.Name, x.TenantId, x.Tenant?.Name ?? string.Empty, x.CreatedOnUtc));
        return Ok(ApiResponseFactory.Success(summaries,"Billing cycles retrieved."));
    }
    #endregion

    #region Get
    /// <summary>
    /// Gets a Billing Cycle by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.BillingCyclesRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById( Guid id, CancellationToken cancellationToken)
    {
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Get the Billing Cycle by ID for the active tenant.
        var billingCycle =await _billingCycles.GetByIdAsync(id,tenantId,cancellationToken);
        if (billingCycle is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Billing cycle not found."));
        }
        // Map the Billing Cycle entity to a summary response.
        var summary = new BillingCycleSummary(billingCycle.Id,billingCycle.Name,billingCycle.TenantId, billingCycle.Tenant?.Name ?? string.Empty, billingCycle.CreatedOnUtc);
        return Ok(ApiResponseFactory.Success(summary,"Billing cycle retrieved."));
    }
    #endregion

    #region Create

    /// <summary>
    /// Creates a new Billing Cycle for the caller's active tenant.
    /// The billing cycle name must be unique within the tenant.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.BillingCyclesWrite)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateBillingCycleRequest request,CancellationToken cancellationToken)
    {
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Validate that the Billing Cycle name is provided.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Billing cycle name is required." });
        }
        var name = request.Name.Trim();
        // Check whether the same name already exists for the tenant.
        var nameExists = await _billingCycles.ExistsByNameAsync(tenantId,name,cancellationToken: cancellationToken);
        // Return Conflict if a Billing Cycle with the same name already exists.
        if (nameExists)
        {
            return Conflict(new { message =$"A billing cycle with the name '{name}' already exists." });
        }
        var billingCycle = new Domain.Entities.BillingCycle{ Id = Guid.NewGuid(), TenantId = tenantId, Name = name, CreatedOnUtc = DateTime.UtcNow, CreatedById = User.GetUserId(), Deleted = false };
        // Add the new Billing Cycle to the database context.
        await _billingCycles.AddAsync(billingCycle,cancellationToken);
        // Save the new Billing Cycle to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var createdBillingCycle = await _billingCycles.GetByIdAsync(billingCycle.Id,tenantId,cancellationToken);
        // Retrieve the created Billing Cycle to get tenant details.
        var summary = new BillingCycleSummary( billingCycle.Id, billingCycle.Name, billingCycle.TenantId, createdBillingCycle?.Tenant?.Name ?? string.Empty, billingCycle.CreatedOnUtc);
        return StatusCode(StatusCodes.Status201Created,ApiResponseFactory.Success(summary,"Billing cycle created."));
    }
    #endregion

    #region Update

    /// <summary>
    /// Updates an existing Billing Cycle.
    /// The billing cycle name must be unique within the tenant,
    /// excluding the billing cycle currently being updated.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.BillingCyclesWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id,[FromBody] UpdateBillingCycleRequest request,CancellationToken cancellationToken)
    {
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Validate that the Billing Cycle name is provided.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Billing cycle name is required." });
        }
        var name = request.Name.Trim();
        var billingCycle =await _billingCycles.GetByIdAsync(id, tenantId, cancellationToken);
        if (billingCycle is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Billing cycle not found."));
        }
        // Check whether the same name already exists for the tenant,
        // excluding the Billing Cycle currently being updated.
        var nameExists = await _billingCycles.ExistsByNameAsync(tenantId, name, excludeId: id, cancellationToken: cancellationToken);
        // Return Conflict if another Billing Cycle has the same name.
        if (nameExists)
        {
            return Conflict(new { message =$"A billing cycle with the name '{name}' already exists." });
        }
        billingCycle.Name = name;
        billingCycle.UpdatedOnUtc = DateTime.UtcNow;
        billingCycle.UpdatedById = User.GetUserId();
        // Mark the Billing Cycle as updated in the database context.
        _billingCycles.Update(billingCycle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Map the updated Billing Cycle to the response summary.
        var summary = new BillingCycleSummary(billingCycle.Id, billingCycle.Name,billingCycle.TenantId,billingCycle.Tenant?.Name ?? string.Empty,billingCycle.CreatedOnUtc);
        return Ok(ApiResponseFactory.Success(summary,"Billing cycle updated."));
    }
    #endregion

    #region Delete

    /// <summary>
    /// Soft deletes a Billing Cycle.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.BillingCyclesDelete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id,CancellationToken cancellationToken)
    {
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode( StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden( "No active tenant for the caller."));
        }
        var billingCycle =await _billingCycles.GetByIdAsync( id,tenantId, cancellationToken);
        if (billingCycle is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Billing cycle not found."));
        }
        billingCycle.Deleted = true;
        // Update the audit information.
        billingCycle.UpdatedOnUtc = DateTime.UtcNow;
        billingCycle.UpdatedById = User.GetUserId();
        // Mark the Billing Cycle as updated in the database context.
        _billingCycles.Update(billingCycle);
        // Save the changes to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponseFactory.Success(new { billingCycleId = billingCycle.Id },"Billing cycle deleted."));
    }
    #endregion
}
