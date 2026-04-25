using HMS.Application.Abstractions.Tenant;
using HMS.Infrastructure.Persistence;
using HMS.Infrastructure.Services;
using HMS.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HMS.Application.Abstractions.Auth;
using HMS.Infrastructure.Authentication;
using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Services;
namespace HMS.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            // 🧠 Tenant + User
            services.AddScoped<ITenantProvider, TenantProvider>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // 🔐 Auth
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtService, JwtService>();

            // 💾 DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // 💣 الربط المهم
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            return services;
        }
    }
}