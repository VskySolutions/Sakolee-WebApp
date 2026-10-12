using Sakolee.Api.Models.Leads;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Leads;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Abstractions.Tenancy;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Lead Files: students of the caller's tenant who are not currently sitting in a running class, so
/// the Families area can list them as leads and follow up. Read-only — the rows are joined from
/// <see cref="ILeadService"/> (student + linked Person + household), filtered and paged here.
/// <para>Reads reuse the Families read permission: a lead is a family-side follow-up list, not a new
/// capability of its own.</para>
/// </summary>
[ApiController]
[Authorize]
[Route("/api/admin/leads")]
[Produces("application/json")]
[Tags("Leads")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class LeadsController : ControllerBase
{
    #region Field Declarations

    private readonly ILeadService _leads;
    private readonly IUserRepository _users;
    private readonly ITenantContext _tenantContext;

    #endregion

    #region Constructor

    public LeadsController(ILeadService leads, IUserRepository users, ITenantContext tenantContext)
    {
        _leads = leads;
        _users = users;
        _tenantContext = tenantContext;
    }

    #endregion

    #region API Endpoints

    /// <summary>
    /// Retrieves a paginated list of leads — students not enrolled in a running class — scoped to the
    /// caller's active tenant, with optional studio location, contact name, student name, and email filters.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.FamiliesRead)]
    [ProducesResponseType<ApiResponse<IEnumerable<LeadSummary>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] Guid? studioLocationId = null,
        [FromQuery] string? contactFirstName = null,
        [FromQuery] string? contactLastName = null,
        [FromQuery] string? studentFirstName = null,
        [FromQuery] string? studentLastName = null,
        [FromQuery] string? email = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var tenantId = _tenantContext.IsResolved ? _tenantContext.TenantId : (Guid?)null;
        var (items, total) = await _leads.ListAsync(
            new LeadListQuery(
                tenantId, studioLocationId, contactFirstName, contactLastName,
                studentFirstName, studentLastName, email, sortBy, descending, page, limit),
            cancellationToken);

        // Batch-resolve actor display names for the audit columns, then project to summaries.
        var names = await ResolveActorNamesAsync(items.SelectMany(i => new[] { i.CreatedById, i.UpdatedById }), cancellationToken);
        var summaries = items.Select(i => ToSummary(i, names));

        return Ok(ApiResponseFactory.Paginated(summaries, "Leads retrieved.", page, limit, total));
    }

    #endregion

    #region Mapping Helpers

    private async Task<IReadOnlyDictionary<Guid, string>> ResolveActorNamesAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken)
        => await _users.GetFullNamesAsync(ids.Where(id => id.HasValue).Select(id => id!.Value), cancellationToken);

    private static string? NameOf(IReadOnlyDictionary<Guid, string> names, Guid? id)
        => id.HasValue && names.TryGetValue(id.Value, out var name) ? name : null;

    /// <summary>Maps a service lead row to its list-row summary shape.</summary>
    private static LeadSummary ToSummary(LeadListItem i, IReadOnlyDictionary<Guid, string> names) => new(
        i.StudentId, i.PersonId, i.FamilyId,
        i.StudentFirstName, i.StudentLastName, i.Email, i.CellPhone,
        i.ContactFirstName, i.ContactLastName, i.ContactEmail, i.ContactPhone,
        i.FamilyName, i.StudioLocationId, i.StudioLocationName, i.AdmissionDate,
        NameOf(names, i.CreatedById), i.CreatedOnUtc,
        NameOf(names, i.UpdatedById), i.UpdatedOnUtc);

    #endregion
}
