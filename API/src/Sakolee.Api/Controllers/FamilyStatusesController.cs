using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sakolee.Api.Security;
using Sakolee.Domain.Entities;
using Sakolee.Infrastructure.Persistence;

namespace Sakolee.Api.Controllers
{
    [Route("api/admin/family-statuses")]
    [ApiController]
    [Authorize]
    public class FamilyStatusesController : ControllerBase
    {
        #region Private Fields & Constructor

        private readonly SakoleeDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="FamilyStatusesController"/> class.
        /// </summary>
        /// <param name="context">The database context instance.</param>
        public FamilyStatusesController(SakoleeDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #endregion

        #region GetFamilyStatuses Endpoint

        /// <summary>
        /// Retrieves a paginated and filtered list of family status records.
        /// </summary>
        /// <param name="page">The current page index (defaults to 1).</param>
        /// <param name="limit">The maximum number of records per page (defaults to 20).</param>
        /// <param name="search">Optional search term for filtering by status name.</param>
        /// <param name="tenantId">Optional tenant identifier filter.</param>
        /// <returns>A collection of family statuses along with pagination metadata.</returns>
        [HttpGet]
        public async Task<IActionResult> GetFamilyStatuses(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string search = null,
            [FromQuery] Guid? tenantId = null,
            [FromQuery] string sortBy = null,
            [FromQuery] bool descending = false)
        {
            // Initialize base queryable data source context (by default filtering out soft-deleted records).
            // The ambient tenant query filter (SakoleeDbContext) already restricts this to the caller's
            // active/viewed tenant, so switching tenants changes what this query returns automatically.
            var query = _context.FamilyStatuses.Where(x => !x.IsDeleted).AsQueryable();

            // A client-supplied tenantId is only honoured for a Super Admin explicitly cross-tenant filtering;
            // for everyone else it is ignored (the ambient filter already pins them to their own tenant).
            if (tenantId.HasValue && User.IsSuperAdmin())
            {
                query = query.Where(x => x.TenantId == tenantId.Value);
            }

            // Apply case-insensitive partial text search filter on the name property if specified
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Name.Contains(search));
            }

            // Retrieve total count of records matching current filters prior to pagination
            var totalRecords = await query.CountAsync();

            // Flag to track whether custom column sorting was successfully applied
            bool isSorted = false;

            // Handle dynamic column sorting based on client request parameters
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);
                        isSorted = true;
                        break;
                    case "createdon":
                    case "createdat":
                        query = descending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn);
                        isSorted = true;
                        break;
                    case "familystatusid":
                        query = descending ? query.OrderByDescending(x => x.FamilyStatusId) : query.OrderBy(x => x.FamilyStatusId);
                        isSorted = true;
                        break;
                }
            }

            // Fallback to default sorting (newest records first) if no valid sorting column was provided
            if (!isSorted)
            {
                query = query.OrderByDescending(x => x.CreatedOn);
            }

            // Apply pagination slicing (skip and take) on the finalized sorted and filtered query
            var data = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            // Return structured response containing data payload and pagination meta information
            return Ok(new
            {
                data = data,
                meta = new { totalRecords = totalRecords }
            });
        }

        #endregion

        #region GetById Endpoint

        /// <summary>
        /// Retrieves a specific family status record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique GUID of the family status.</param>
        /// <returns>The family status entity if found; otherwise, a NotFound response.</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _context.FamilyStatuses.FirstOrDefaultAsync(x => x.FamilyStatusId == id && !x.IsDeleted);

            if (item == null)
            {
                return NotFound(new { message = "Family status record not found." });
            }

            return Ok(item);
        }

        #endregion

        #region Create Endpoint

        /// <summary>
        /// Creates a new family status record.
        /// </summary>
        /// <param name="model">The family status creation payload.</param>
        /// <returns>The newly created family status entity.</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FamilyStatus model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Invalid request payload provided." });
            }

            if (User.GetActiveTenantId() is not { } activeTenantId)
            {
                return Unauthorized(new { message = "Tenant could not be resolved." });
            }

            model.FamilyStatusId = Guid.NewGuid();
            model.TenantId = activeTenantId;
            model.CreatedOn = DateTime.UtcNow;
            model.IsDeleted = false;

            // Persist the new entity changes to the database
            await _context.FamilyStatuses.AddAsync(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        #endregion

        #region Update Endpoint

        /// <summary>
        /// Updates an existing family status record details.
        /// </summary>
        /// <param name="id">The unique GUID of the record to update.</param>
        /// <param name="model">The updated model data.</param>
        /// <returns>The updated family status entity.</returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FamilyStatus model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Invalid update payload provided." });
            }

            var existing = await _context.FamilyStatuses.FirstOrDefaultAsync(x => x.FamilyStatusId == id && !x.IsDeleted);
            if (existing == null)
            {
                return NotFound(new { message = "Family status record not found." });
            }

            // TenantId is not editable via update — it is stamped once at creation from the caller's
            // active tenant, and the ambient query filter already scoped `existing` to that tenant.
            existing.Name = model.Name;

            _context.FamilyStatuses.Update(existing);
            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        #endregion

        #region Delete Endpoint

        /// <summary>
        /// Soft deletes an existing family status record by setting IsDeleted to true.
        /// </summary>
        /// <param name="id">The unique GUID of the record to delete.</param>
        /// <returns>Success response if deleted.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.FamilyStatuses.FirstOrDefaultAsync(x => x.FamilyStatusId == id && !x.IsDeleted);
            if (existing == null)
            {
                return NotFound(new { message = "Family status record not found." });
            }

            // Perform Soft Delete
            existing.IsDeleted = true;

            _context.FamilyStatuses.Update(existing);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Family status deleted successfully." });
        }

        #endregion
    }
}