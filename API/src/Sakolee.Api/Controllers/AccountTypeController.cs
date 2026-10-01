using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models.AccountType;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Provides administrative operations for Account Type records.
/// Account Type records are scoped to the caller's active tenant.
/// </summary>
[ApiController]
[Authorize]
[Route("/api/admin/account-types")]
[Produces("application/json")]
[Tags("Account Type")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class AccountTypeController : ControllerBase
{
    #region Fields

    private readonly IAccountTypeRepository _accountTypes;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Account Type controller.
    /// </summary>
    public AccountTypeController(IAccountTypeRepository accountTypes,IUnitOfWork unitOfWork,IUserRepository users)
    {
        _accountTypes = accountTypes;
        _unitOfWork = unitOfWork;
        _users = users;
    }

    #endregion

    #region List

    /// <summary>
    /// Gets Account Type records for the caller's active tenant.
    /// Supports searching, filtering, sorting, and pagination.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.AccountTypesRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? name = null,[FromQuery] bool showDeleted = false,[FromQuery] bool? active = null,[FromQuery] string? search = null,[FromQuery] string? sortBy = null,[FromQuery] bool descending = false,[FromQuery] int page = 1,[FromQuery] int limit = 20,CancellationToken cancellationToken = default)
    {
        // Get the active tenant from the current user's claims.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Ensure page and limit values are within valid ranges.
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);
        // Retrieve Account Type records using the requested
        // filters, search, sorting, and pagination options.
        var (accountTypes, total) =await _accountTypes.ListByTenantAsync(tenantId,name,showDeleted,active,search, sortBy,descending,page,limit,cancellationToken);
        // Retrieve display names for users referenced by
        // CreatedById and UpdatedById audit fields.
        var nameOf = await AuditNamesAsync(accountTypes,cancellationToken);
        // Convert Account Type entities into API response summaries.
        var summaries = accountTypes.Select(x => ToSummary(x, nameOf)).ToList();
        // Return the records along with pagination information.
        return Ok(ApiResponseFactory.Paginated(summaries,"Account Type records retrieved.",page,limit,total));
    }

    #endregion

    #region Get

    /// <summary>
    /// Gets an Account Type record by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.AccountTypesRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id,CancellationToken cancellationToken)
    {
        // Get the active tenant from the current user's claims.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Retrieve the Account Type only if it belongs to
        // the current tenant and is not deleted.
        var accountType =await _accountTypes.GetByIdAsync(id,tenantId,cancellationToken);
        // Return 404 when the requested record does not exist.
        if (accountType is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Account Type record not found."));
        }
        // Retrieve display names for the audit fields.
        var nameOf = await AuditNamesAsync(new[] { accountType },cancellationToken);
        // Convert the entity into the API response model.
        var summary = ToSummary(accountType,nameOf);
        // Return the requested Account Type record.
        return Ok(ApiResponseFactory.Success(summary,"Account Type record retrieved."));
    }

    #endregion

    #region Create

    /// <summary>
    /// Creates a new Account Type record.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.AccountTypesWrite)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateAccountTypeRequest request,CancellationToken cancellationToken)
    {
        // Get the active tenant from the current user's claims.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Validate that the Account Type name is provided.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Account Type name is required." });
        }
        // Remove leading and trailing spaces from the name.
        var name = request.Name.Trim();
        // Check whether another non-deleted Account Type
        // with the same name already exists for this tenant.
        var nameExists =await _accountTypes.ExistsByNameAsync(tenantId,name,cancellationToken: cancellationToken);
        // Prevent duplicate Account Type names.
        if (nameExists)
        {
            return Conflict(new {message =$"Account Type record with the name '{name}' already exists."});
        }
        // Create a new Account Type entity.
        var accountType = new Domain.Entities.AccountType{ Id = Guid.NewGuid(),TenantId = tenantId, Name = name, Active = request.Active };
        // Add the new entity to the DbContext.
        await _accountTypes.AddAsync(accountType,cancellationToken);
        // Persist the new Account Type to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Retrieve the newly created record so that the response
        // contains the latest entity and related tenant information.
        var createdAccountType =await _accountTypes.GetByIdAsync(accountType.Id,tenantId,cancellationToken);
        // Use the retrieved entity when available.
        // Otherwise, use the newly created entity as a fallback.
        var created =createdAccountType ?? accountType;
        // Retrieve display names for audit fields.
        var nameOf = await AuditNamesAsync(new[] { created },cancellationToken);
        // Convert the created entity into the API response model.
        var summary = ToSummary(created,nameOf);
        // Return the newly created Account Type.
        return StatusCode(StatusCodes.Status201Created,ApiResponseFactory.Success(summary,"Account Type record created."));
    }

    #endregion

    #region Update

    /// <summary>
    /// Updates an existing Account Type record.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.AccountTypesWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id,[FromBody] UpdateAccountTypeRequest request,CancellationToken cancellationToken)
    {
        // Get the active tenant from the current user's claims.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Validate that the Account Type name is provided.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Account Type name is required." });
        }
        // Remove leading and trailing spaces from the name.
        var name = request.Name.Trim();
        // Retrieve the existing Account Type for the current tenant.
        var accountType =await _accountTypes.GetByIdAsync(id,tenantId,cancellationToken);
        // Return 404 when the record does not exist.
        if (accountType is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Account Type record not found."));
        }
        // Check whether another Account Type already uses
        // the requested name, excluding the current record.
        var nameExists =await _accountTypes.ExistsByNameAsync(tenantId,name, excludeId: id, cancellationToken: cancellationToken);
        // Prevent duplicate Account Type names.
        if (nameExists)
        {
            return Conflict(new { message =$"An Account Type record with the name '{name}' already exists." });
        }
        // Update the Account Type propertie
        accountType.Name = name;
        accountType.Active = request.Active;
        // Mark the entity as modified in the DbContext.
        _accountTypes.Update(accountType);
        // Save the updated record to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Retrieve display names for the audit fields.
        var nameOf = await AuditNamesAsync(new[] { accountType },cancellationToken);
        // Convert the updated entity into the API response model.
        var summary = ToSummary(accountType,nameOf);
        // Return the updated Account Type record.
        return Ok(ApiResponseFactory.Success(summary,"Account Type record updated."));
    }

    #endregion

    #region Delete

    /// <summary>
    /// Soft deletes an Account Type record.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.AccountTypesDelete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id,CancellationToken cancellationToken)
    {
        // Get the active tenant from the current user's claims.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Retrieve the Account Type for the current tenant.
        var accountType =await _accountTypes.GetByIdAsync(id,tenantId,cancellationToken);
        // Return 404 when the record does not exist.
        if (accountType is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Account Type record not found."));
        }
        // Mark the record as deleted instead of physically
        // removing it from the database.
        accountType.Deleted = true;
        accountType.DeletedOnUtc = DateTime.UtcNow;
        // Mark the entity as modified in the DbContext.
        _accountTypes.Update(accountType);
        // Persist the soft-delete changes to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Return the identifier of the deleted record.
        return Ok(ApiResponseFactory.Success(new { accountTypeId = accountType.Id },"Account Type record deleted."));
    }

    #endregion

    #region Summary

    /// <summary>
    /// Maps an Account Type entity to its API response summary.
    /// </summary>
    private static AccountTypeSummary ToSummary(Domain.Entities.AccountType accountType,Func<Guid?, string?> nameOf)
    {
        // Create the API summary using entity and audit information.
        return new AccountTypeSummary(accountType.Id,accountType.Name,accountType.Active,accountType.Deleted,accountType.TenantId,accountType.Tenant?.Name ?? string.Empty,nameOf(accountType.CreatedById),accountType.CreatedOnUtc,nameOf(accountType.UpdatedById),accountType.UpdatedOnUtc);
    }

    #endregion

    #region Audit Names

    /// <summary>
    /// Retrieves display names for users referenced by audit fields.
    /// </summary>
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<Domain.Entities.AccountType> rows,CancellationToken cancellationToken)
    {
        // Collect CreatedById and UpdatedById values from all records.
        var ids = rows.SelectMany(x => new[]{ x.CreatedById,x.UpdatedById }).Where(id => id.HasValue).Select(id => id!.Value).Distinct();
        // Retrieve the user display names for the collected IDs.
        var names =await _users.GetFullNamesAsync(ids,cancellationToken);
        // Return a function that resolves a user ID
        // to the corresponding display name.
        return id =>id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }

    #endregion
}