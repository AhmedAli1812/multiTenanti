using HMS.SharedKernel.Primitives;
using HMS.SharedKernel.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HMS.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Abstract base DbContext — shared across all modules.
/// Applies global tenant query filters. Module configurations applied by HmsDbContext.
/// </summary>
public abstract class ApplicationDbContextBase : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    protected ApplicationDbContextBase(
        DbContextOptions options,
        ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    // ── Transaction support ───────────────────────────────────────────────────
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => await Database.BeginTransactionAsync(ct);

    // ── Model configuration ───────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ApplyGlobalFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    // ── Global query filters ──────────────────────────────────────────────────
    protected void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (!typeof(ITenantEntity).IsAssignableFrom(clrType)) continue;

            var method = typeof(ApplicationDbContextBase)
                .GetMethod(nameof(ApplyTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(clrType);

            method.Invoke(this, [modelBuilder]);
        }
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class
    {
        var isSoftDeletable = typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity));

        if (isSoftDeletable)
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
                (
                    _tenantProvider.IsSuperAdmin()
                    || (
                        _tenantProvider.TryGetTenantId() != null
                        && EF.Property<Guid?>(e, "TenantId") == _tenantProvider.TryGetTenantId()
                    )
                )
                && EF.Property<bool>(e, "IsDeleted") == false
            );
        }
        else
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
                _tenantProvider.IsSuperAdmin()
                || (
                    _tenantProvider.TryGetTenantId() != null
                    && EF.Property<Guid?>(e, "TenantId") == _tenantProvider.TryGetTenantId()
                )
            );
        }
    }
}
