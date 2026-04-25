using HMS.Application.Abstractions.Tenant;
using HMS.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HMS.Infrastructure.Persistence.GlobalFilters
{
    public static class TenantQueryFilter
    {
        public static void Apply(ModelBuilder modelBuilder, ITenantProvider tenantProvider)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(TenantQueryFilter)
                        .GetMethod(nameof(SetTenantFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(null, new object[] { modelBuilder, tenantProvider });
                }
            }
        }

        private static void SetTenantFilter<TEntity>(
            ModelBuilder modelBuilder,
            ITenantProvider tenantProvider)
            where TEntity : TenantEntity
        {
            modelBuilder.Entity<TEntity>()
                .HasQueryFilter(e =>
                    // 🔥 Super Admin يشوف كل حاجة
                    tenantProvider.IsSuperAdmin()

                    // 🔥 Tenant filter (SAFE)
                    || EF.Property<Guid?>(e, "TenantId") ==
                       tenantProvider.TryGetTenantId()
                );
        }
    }
}