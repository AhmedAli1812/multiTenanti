using HMS.Application.Abstractions.Tenant;
using HMS.Domain.Entities.Base;
using HMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
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
            // ⚠️ مهم جدًا: خد القيمة مرة واحدة
            var tenantId = tenantProvider.TryGetTenantId();

            Expression<Func<TEntity, bool>> filter;

            // 🔥 Super Admin → يشوف كل حاجة
            if (tenantProvider.IsSuperAdmin())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
                {
                    filter = e => !((ISoftDeletable)e).IsDeleted;
                }
                else
                {
                    filter = e => true;
                }
            }
            else
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
                {
                    filter = e =>
                        EF.Property<Guid?>(e, "TenantId") == tenantId &&
                        EF.Property<bool>(e, "IsDeleted") == false;
                }
                else
                {
                    filter = e =>
                        EF.Property<Guid?>(e, "TenantId") == tenantId;
                }
            }

            modelBuilder.Entity<TEntity>().HasQueryFilter(filter);
        }
    }
}