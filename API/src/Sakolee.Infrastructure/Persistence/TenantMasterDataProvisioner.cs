using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence;

/// <summary>
/// Copies the template tenant's <c>IsSystem</c> master rows into a new tenant. Each copy gets a new id and
/// the new tenant's id; every other column (Name, Active, Code, IsSystem, …) is carried over as-is.
/// </summary>
internal sealed class TenantMasterDataProvisioner : ITenantMasterDataProvisioner
{
    #region Fields

    private readonly SakoleeDbContext _dbContext;
    private readonly IConfiguration _configuration;

    #endregion

    #region Constructor

    public TenantMasterDataProvisioner(SakoleeDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    #endregion

    #region Copy

    public async Task<int> CopyDefaultsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var identifier = FirstNonBlank(
            _configuration["TenantProvisioning:TemplateTenantIdentifier"],
            _configuration["Bootstrap:TenantIdentifier"],
            "system");

        var templateId = await _dbContext.Tenants
            .Where(t => t.Identifier == identifier)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (templateId is not { } source || source == tenantId)
        {
            return 0;
        }

        // Query filters are bypassed (a resolved tenant context would hide the template tenant's rows),
        // so the soft-delete predicate is re-applied on every query.
        var count = 0;

        count += (await CopyAsync(
            _dbContext.AccountTypes.IgnoreQueryFilters().Where(x => x.TenantId == source && x.IsSystem && !x.Deleted),
            x => x.Id, (x, id) => { x.Id = id; x.TenantId = tenantId; }, cancellationToken)).Count;

        count += (await CopyAsync(
            _dbContext.BillingCycles.IgnoreQueryFilters().Where(x => x.TenantId == source && x.IsSystem && !x.Deleted),
            x => x.Id, (x, id) => { x.Id = id; x.TenantId = tenantId; }, cancellationToken)).Count;

        count += (await CopyAsync(
            _dbContext.BillingMethod.IgnoreQueryFilters().Where(x => x.TenantId == source && x.IsSystem && !x.Deleted),
            x => x.Id, (x, id) => { x.Id = id; x.TenantId = tenantId; }, cancellationToken)).Count;

        count += (await CopyAsync(
            _dbContext.ClassCategories.IgnoreQueryFilters().Where(x => x.TenantId == source && x.IsSystem && !x.Deleted),
            x => x.Id, (x, id) => { x.Id = id; x.TenantId = tenantId; }, cancellationToken)).Count;

        count += (await CopyAsync(
            _dbContext.FamilyStatuses.IgnoreQueryFilters().Where(x => x.TenantId == source && x.IsSystem && !x.Deleted),
            x => x.FamilyStatusId, (x, id) => { x.FamilyStatusId = id; x.TenantId = tenantId; }, cancellationToken)).Count;

        count += (await CopyAsync(
            _dbContext.HearAboutUs.IgnoreQueryFilters().Where(x => x.TenantId == source && x.IsSystem && !x.Deleted),
            x => x.Id, (x, id) => { x.Id = id; x.TenantId = tenantId; }, cancellationToken)).Count;

        count += (await CopyAsync(
            _dbContext.MembershipType.IgnoreQueryFilters().Where(x => x.TenantId == source && x.IsSystem && !x.Deleted),
            x => x.Id, (x, id) => { x.Id = id; x.TenantId = tenantId; }, cancellationToken)).Count;

        count += (await CopyAsync(
            _dbContext.StudentGradeLevel.IgnoreQueryFilters().Where(x => x.TenantId == source && x.IsSystem && !x.Deleted),
            x => x.Id, (x, id) => { x.Id = id; x.TenantId = tenantId; }, cancellationToken)).Count;

        count += (await CopyAsync(
            _dbContext.TShirtSizes.IgnoreQueryFilters().Where(x => x.TenantId == source && x.IsSystem && !x.Deleted),
            x => x.Id, (x, id) => { x.Id = id; x.TenantId = tenantId; }, cancellationToken)).Count;

        // Locations before Class Rooms: a room is re-pointed at its location's copy. A room whose location
        // was not copied (not IsSystem) is skipped — it would otherwise reference another tenant's location.
        var locationIds = await CopyAsync(
            _dbContext.Locations.IgnoreQueryFilters().Where(x => x.TenantId == source && x.IsSystem && !x.Deleted),
            x => x.Id, (x, id) => { x.Id = id; x.TenantId = tenantId; }, cancellationToken);
        count += locationIds.Count;

        var copiedLocations = locationIds.Keys.ToList();
        count += (await CopyAsync(
            _dbContext.ClassRooms.IgnoreQueryFilters()
                .Where(x => x.TenantId == source && x.IsSystem && !x.Deleted && copiedLocations.Contains(x.LocationId)),
            x => x.Id, (x, id) => { x.Id = id; x.TenantId = tenantId; x.LocationId = locationIds[x.LocationId]; },
            cancellationToken)).Count;

        return count;
    }

    /// <summary>
    /// Loads the template rows untracked, re-keys each one via <paramref name="retarget"/> and stages it as a
    /// new row. Returns template id → copy id, for re-pointing rows that reference these.
    /// </summary>
    private async Task<Dictionary<Guid, Guid>> CopyAsync<T>(
        IQueryable<T> templateRows, Func<T, Guid> getId, Action<T, Guid> retarget, CancellationToken cancellationToken)
        where T : AuditableEntity
    {
        var rows = await templateRows.AsNoTracking().ToListAsync(cancellationToken);
        var ids = new Dictionary<Guid, Guid>(rows.Count);
        var now = DateTime.UtcNow;

        foreach (var row in rows)
        {
            var newId = Guid.NewGuid();
            ids[getId(row)] = newId;
            retarget(row, newId);

            // CreatedOnUtc / CreatedById are stamped by SaveChanges; the template's update stamp is not ours.
            row.UpdatedOnUtc = now;
            row.UpdatedById = null;
            row.DeletedOnUtc = null;
            _dbContext.Add(row);
        }

        return ids;
    }

    private static string FirstNonBlank(params string?[] values)
        => values.First(v => !string.IsNullOrWhiteSpace(v))!;

    #endregion
}
