using HMS.Identity.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Identity.Infrastructure;

/// <summary>Identity module DI registration.</summary>
public static class IdentityModuleExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IPasswordHasher, Authentication.PasswordHasher>();
        services.AddScoped<IJwtService,     Authentication.JwtService>();
        // IIdentityDbContext registered in HMS.Persistence.PersistenceExtensions
        // FluentValidation registered in Program.cs via AddValidatorsFromAssembly
        return services;
    }
}
