using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models;
using Sakolee.Api.Models.Session;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Auditing;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Controller managing class session master records for multi-tenant configurations.
/// </summary>
[ApiController]
[Route("/api/admin/sessions")]
[Produces("application/json")]
[Tags("Sessions")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status500InternalServerError)]
public sealed class SessionsController : ControllerBase
{
    #region Field Declarations

    private readonly IClassSessionRepository _sessions;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditTrailService _audit;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionsController"/> class.
    /// </summary>
    /// <param name="sessions">The class session repository instance.</param>
    /// <param name="users">The user repository instance.</param>
    /// <param name="unitOfWork">The unit of work instance for transaction management.</param>
    /// <param name="audit">The audit trail service instance.</param>
    public SessionsController(
        IClassSessionRepository sessions,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IAuditTrailService audit)
    {
        _sessions = sessions;
        _users = users;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    #endregion

    #region API Endpoints

    #region Create Endpoint

    /// <summary>
    /// Creates a new class session record within the active tenant context.
    /// </summary>
    /// <param name="request">The create session request payload containing session details.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Returns the created session detail response object.</returns>
    [HttpPost]
    [RequirePermission(Permissions.SessionsWrite)]
    [ProducesResponseType<ApiResponse<SessionDetail>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSessionRequest request, CancellationToken cancellationToken)
    {
        // Validate if the request body name is null or whitespace
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(ApiResponseFactory.Error(
                ApiErrorCodes.ValidationFailed, "Validation failed.", "Session name is required."));
        }

        // Check if a session with the exact same name already exists in the system
        if (await _sessions.ExistsAsync(request.Name, cancellationToken))
        {
            return BadRequest(ApiResponseFactory.Error(
                ApiErrorCodes.ValidationFailed, "Validation failed.", "A session with this name already exists."));
        }

        // Strictly fetch the active tenant identifier from the current user session context
        var tenantId = User.GetActiveTenantId() ?? Guid.Empty;
        if (tenantId == Guid.Empty)
        {
            return BadRequest(ApiResponseFactory.Error(
                ApiErrorCodes.ValidationFailed, "Validation failed.", "Active tenant ID could not be determined."));
        }

        // Initialize a new ClassSessions entity instance with the active tenant ID
        var session = new ClassSessions
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = User.GetUserId().ToString()
        };

        // Persist the new record to the database via repository and unit of work
        await _sessions.AddAsync(session, cancellationToken);
        await _audit.AddAsync(nameof(ClassSessions), session.Id.ToString(), "Created", details: session.Name, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map the created entity to a detailed response DTO matching constructor parameter sequence
        var detail = new SessionDetail(
            session.Id,
            session.Name,
            !session.IsDeleted,
            session.CreatedBy,
            session.UpdatedBy,
            session.CreatedOn,
            session.UpdatedOn,
            session.TenantId,
            session.Tenant?.Name);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponseFactory.Success(detail, "Session created."));
    }

    #endregion

    #region List Endpoint

    /// <summary>
    /// Retrieves a paginated, filtered, and sorted list of class session records.
    /// </summary>
    /// <param name="page">The page number for pagination (default is 1).</param>
    /// <param name="limit">The number of records per page (default is 20).</param>
    /// <param name="search">Optional search term to filter sessions by name.</param>
    /// <param name="tenantId">Optional tenant identifier filter (restricted to super admin).</param>
    /// <param name="isActive">Optional filter to retrieve records based on active status.</param>
    /// <param name="sortBy">Optional column name to sort the results by.</param>
    /// <param name="descending">Indicates whether sorting should be in descending order (default is true).</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Returns a paginated list of session summaries.</returns>
    [HttpGet]
    [RequirePermission(Permissions.SessionsRead)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure valid pagination boundary values
            page = Math.Max(1, page);
            limit = Math.Clamp(limit, 1, 100);

            // Scope tenant filter for super admin users if explicitly specified in query parameters
            Guid? scopeTenant = User.IsSuperAdmin() && tenantId is { } tid ? tid : null;

            // Fetch filtered, sorted, and paginated records from the repository
            var (items, total) = await _sessions.ListAsync(
                search, scopeTenant, isActive, new SortRequest(sortBy, descending), page, limit,
                cancellationToken: cancellationToken);

            // Project database model items into summary DTOs including Tenant information matching exact constructor signature
            var summaries = items.Select(s => new SessionSummary(
                s.Id,
                s.Name,
                !s.IsDeleted,
                s.CreatedBy,
                s.UpdatedBy,
                s.CreatedOn,
                s.UpdatedOn,
                s.TenantId,
                s.Tenant != null ? s.Tenant.Name : string.Empty
            ));

            return Ok(ApiResponseFactory.Paginated(summaries, "Sessions retrieved.", page, limit, total));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = ex.Message,
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    #endregion

    #region GetById Endpoint

    /// <summary>
    /// Retrieves a specific class session record by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the session.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Returns the detailed session record if found.</returns>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.SessionsRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        // Load the session entity handling tenant scoping rules
        var session = await LoadAsync(id, cancellationToken);
        if (session is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Session not found."));
        }

        // Map entity details to response DTO
        var detail = new SessionDetail(
            session.Id,
            session.Name,
            !session.IsDeleted,
            session.CreatedBy,
            session.UpdatedBy,
            session.CreatedOn,
            session.UpdatedOn,
            session.TenantId,
            session.Tenant?.Name);

        return Ok(ApiResponseFactory.Success(detail, "Session retrieved."));
    }

    #endregion

    #region Update Endpoint

    /// <summary>
    /// Updates an existing class session record by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the session to update.</param>
    /// <param name="request">The update session request payload containing modified values.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Returns the updated session detail response object.</returns>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.SessionsWrite)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSessionRequest request, CancellationToken cancellationToken)
    {
        var session = await LoadAsync(id, cancellationToken);
        if (session is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Session not found."));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            session.Name = request.Name.Trim();
        }

        if (request.IsActive.HasValue)
        {
            session.IsDeleted = !request.IsActive.Value;
        }

        session.UpdatedOn = DateTime.UtcNow;
        session.UpdatedBy = User.GetUserId().ToString();

        _sessions.Update(session);

        await _audit.AddAsync(nameof(ClassSessions), session.Id.ToString(), "Updated", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var detail = new SessionDetail(
            session.Id,
            session.Name,
            !session.IsDeleted,
            session.CreatedBy,
            session.UpdatedBy,
            session.CreatedOn,
            session.UpdatedOn,
            session.TenantId,
            session.Tenant?.Name);

        return Ok(ApiResponseFactory.Success(detail, "Session updated."));
    }

    #endregion

    #region Delete Endpoint

    /// <summary>
    /// Soft deletes a class session record by updating its IsDeleted flag.
    /// </summary>
    /// <param name="id">The unique identifier of the session to delete.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Returns a success status response upon successful soft deletion.</returns>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.SessionsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        // Verify delete permissions if the caller is not a super admin
        if (!User.IsSuperAdmin() && !User.HasPermission(Permissions.SessionsDelete))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponseFactory.Forbidden("You do not have permission to delete sessions."));
        }

        // Load the target session entity
        var session = await LoadAsync(id, cancellationToken);
        if (session is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Session not found."));
        }

        // Soft delete: set IsDeleted flag to true instead of physical removal
        session.IsDeleted = true;
        session.UpdatedOn = DateTime.UtcNow;

        // Update the entity state in the repository/context and log the audit event
        _sessions.Update(session);
        await _audit.AddAsync(nameof(ClassSessions), session.Id.ToString(), "SoftDeleted", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(new { id }, "Session moved to trash successfully."));
    }

    #endregion

    #endregion

    #region Private Helper Methods

    #region LoadAsync Helper

    /// <summary>
    /// Loads a class session entity based on super admin privileges or standard tenant context filters.
    /// </summary>
    /// <param name="id">The unique identifier of the session.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Returns the matching <see cref="ClassSessions"/> entity if found; otherwise, null.</returns>
    private Task<ClassSessions?> LoadAsync(Guid id, CancellationToken cancellationToken)
        => User.IsSuperAdmin()
            ? _sessions.GetByIdUnscopedAsync(id, cancellationToken)
            : _sessions.GetByIdAsync(id, cancellationToken);

    #endregion

    #endregion
}