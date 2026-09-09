using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Abstractions.Tenancy;
using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class RetentionConfigRepository : IRetentionConfigRepository
{
    private readonly SakoleeDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public RetentionConfigRepository(SakoleeDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public Task<DeletedRecordRetentionConfig?> GetAsync(Guid? tenantId, CancellationToken cancellationToken = default)
    {
        var effective = tenantId ?? (_tenantContext.IsResolved ? _tenantContext.TenantId : (Guid?)null);
        return effective is { } tid
            ? _dbContext.DeletedRecordRetentionConfigs.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.TenantId == tid && !c.Deleted, cancellationToken)
            : _dbContext.DeletedRecordRetentionConfigs.FirstOrDefaultAsync(cancellationToken);
    }

    public Task AddAsync(DeletedRecordRetentionConfig config, CancellationToken cancellationToken = default)
        => _dbContext.DeletedRecordRetentionConfigs.AddAsync(config, cancellationToken).AsTask();

    public void Update(DeletedRecordRetentionConfig config) => _dbContext.DeletedRecordRetentionConfigs.Update(config);
}
