using HMS.SharedKernel.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HMS.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Allows dotnet-ef to instantiate HmsDbContext without running the full host.
///
/// Usage:
///   dotnet ef migrations add InitialModularSchema \
///     --project src/HMS.Persistence \
///     --startup-project src/HMS.API
/// </summary>
public sealed class HmsDbContextDesignTimeFactory
    : IDesignTimeDbContextFactory<HmsDbContext>
{
    public HmsDbContext CreateDbContext(string[] args)
    {
        // Load connection string from HMS.API appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "HMS.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection not found in appsettings.json. " +
                "Ensure HMS.API/appsettings.json is present.");

        var optionsBuilder = new DbContextOptionsBuilder<HmsDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql =>
        {
            sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
            sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "shared");
        });

        // Stub tenant provider — design time only, no HTTP context
        var tenantProvider = new DesignTimeTenantProvider();
        return new HmsDbContext(optionsBuilder.Options, tenantProvider);
    }
}

/// <summary>Stub ITenantProvider used only during migration generation.</summary>
internal sealed class DesignTimeTenantProvider : ITenantProvider
{
    public Guid? TryGetTenantId() => null;
    public bool  IsSuperAdmin()   => true;   // bypass tenant filter for migrations
    public void  SetTenantId(Guid tenantId) { }
}
