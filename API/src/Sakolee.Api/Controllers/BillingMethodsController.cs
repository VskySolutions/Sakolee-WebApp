using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models;
using Sakolee.Api.Models.BankMethods;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Auditing;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Abstractions.Security;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Bank Method Master management for the active tenant.
/// Bank methods belong to the Dance Studio/Tenant that owns them.
/// </summary>
[ApiController]
[Route("/api/admin/billing-methods")]
[Produces("application/json")]
[Tags("Bank Methods")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status500InternalServerError)]
public sealed class BillingMethodsController : ControllerBase
{
    private readonly IBillingMethodRepository _billingMethod;
    private readonly IUserRepository _users;
    private readonly IAuditTrailService _audit;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="BillingMethodsController"/> class.
    /// </summary>
    public BillingMethodsController(
        IBillingMethodRepository billingMethod,
        IUserRepository users,
        IAuditTrailService audit,
        IUnitOfWork unitOfWork)
    {
        _billingMethod = billingMethod;
        _users = users;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    #region List Bank Methods

    /// <summary>
    /// Gets all bank methods belonging to the active tenant.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.BillingMethodsRead)] 
    [ProducesResponseType<ApiResponse<IReadOnlyList<BankMethodSummary>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? active = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        // Ensure valid pagination boundary values
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var tenantId = User.GetActiveTenantId();

        // Fetch paginated, filtered, and sorted records from repository
        var (items, total) = await _billingMethod.ListAsync(
            search, tenantId, active, new SortRequest(sortBy, descending), page, limit,
            cancellationToken: cancellationToken);

        var nameOf = await AuditNamesAsync(items, cancellationToken);

        var summaries = items.Select(billingMethod => ToSummary(billingMethod, nameOf)).ToList();

        return Ok(ApiResponseFactory.Paginated(summaries, "Billing methods retrieved.", page, limit, total));
    }

    #endregion

    #region Get Bank Method By ID

    /// <summary>
    /// Gets a single bank method belonging to the active tenant.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.BillingMethodsRead)] 
    [ProducesResponseType<ApiResponse<BankMethodResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var billingMethod = await _billingMethod.GetByIdAsync(id, cancellationToken);

        if (billingMethod is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Billing method not found."));
        }

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(billingMethod, cancellationToken), "Billing method retrieved."));
    }

    #endregion

    #region Create Bank Method

    /// <summary>
    /// Creates a bank method for the active tenant.
    /// TenantId is assigned automatically by the persistence layer.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.BillingMethodsWrite)] 
    [ProducesResponseType<ApiResponse<BankMethodResponse>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBankMethodRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Billing method name is required."));
        }

        if (name.Length > 100)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Billing method name cannot exceed 100 characters."));
        }

        if (await _billingMethod.NameExistsAsync(name, User.GetActiveTenantId(), cancellationToken: cancellationToken))
        {
            //return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Bank method name already exists.", name));
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Validation failed.", "A Billing method with this name already exists."));
        }

        var bankMethod = new BillingMethod
        {
            Id = Guid.NewGuid(),
            Name = name,
            Active = request.Active

            // TenantId intentionally NOT assigned here.
            // SakoleeDbContext.StampTenant() assigns it
            // from ITenantContext.TenantId.
        };

        await _billingMethod.AddAsync(bankMethod, cancellationToken);

        await _audit.AddAsync(nameof(BillingMethod), bankMethod.Id.ToString(), "Created", details: $"name={bankMethod.Name}", cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ApiResponseFactory.Success(await ToResponseAsync(bankMethod, cancellationToken), "Billing method created."));
    }

    #endregion

    #region Update Bank Method

    /// <summary>
    /// Updates a bank method belonging to the active tenant.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.BillingMethodsWrite)] // Update to BankMethodsWrite if defined in your permissions
    [ProducesResponseType<ApiResponse<BankMethodResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBankMethodRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var billingMethod = await _billingMethod.GetByIdAsync(id, cancellationToken);

        if (billingMethod is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Billing method not found."));
        }

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Billing method name is required."));
        }

        if (name.Length > 100)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Billing method name cannot exceed 100 characters."));
        }

        if (await _billingMethod.NameExistsAsync(name, billingMethod.TenantId, billingMethod.Id, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Validation failed.", "A Billing method with this name already exists."));
        }

        billingMethod.Name = name;
        billingMethod.Active = request.Active;

        _billingMethod.Update(billingMethod);

        await _audit.AddAsync(nameof(BillingMethod), billingMethod.Id.ToString(), "Updated",
            details: $"name={billingMethod.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(billingMethod, cancellationToken), "Billing method updated."));
    }

    #endregion

    #region Delete Bank Method

    /// <summary>
    /// Soft-deletes a bank method belonging to the active tenant.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.BillingMethodsDelete)] // Update to BankMethodsDelete if defined in your permissions
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var billingMethod = await _billingMethod.GetByIdAsync(id, cancellationToken);

        if (billingMethod is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Billing method not found."));
        }

        _billingMethod.Remove(billingMethod);

        await _audit.AddAsync(nameof(BillingMethod), billingMethod.Id.ToString(), "Deleted",
            details: $"name={billingMethod.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(new { id = billingMethod.Id }, "Billing method deleted."));
    }

    #endregion

    #region Helpers

    private bool HasActiveTenant()
        => User.GetActiveTenantId() is not null;

    private IActionResult NoActiveTenant()
        => StatusCode(StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden("No active tenant for the caller."));

    /// <summary>
    /// Resolves user IDs used by audit fields into display names.
    /// </summary>
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<BillingMethod> rows, CancellationToken cancellationToken)
    {
        var ids = rows.SelectMany(billingMethod => new[]
                {
                    billingMethod.CreatedById,
                    billingMethod.UpdatedById
                }).Where(id => id.HasValue).Select(id => id!.Value);

        var names = await _users.GetFullNamesAsync(ids, cancellationToken);

        return id => id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }

    private async Task<BankMethodResponse> ToResponseAsync(BillingMethod billingMethod, CancellationToken cancellationToken)
    {
        var nameOf = await AuditNamesAsync(new[] { billingMethod }, cancellationToken);

        return new BankMethodResponse(
            billingMethod.Id,
            billingMethod.TenantId,
            billingMethod.Name,
            billingMethod.Active,
            nameOf(billingMethod.CreatedById),
            billingMethod.CreatedOnUtc,
            nameOf(billingMethod.UpdatedById),
            billingMethod.UpdatedOnUtc);
    }

    private static BankMethodSummary ToSummary(BillingMethod billingMethod, Func<Guid?, string?> nameOf)
        => new(
            billingMethod.Id,
            billingMethod.TenantId,
            billingMethod.Name,
            billingMethod.Active,
            nameOf(billingMethod.CreatedById),
            billingMethod.CreatedOnUtc,
            nameOf(billingMethod.UpdatedById),
            billingMethod.UpdatedOnUtc);

    #endregion
}