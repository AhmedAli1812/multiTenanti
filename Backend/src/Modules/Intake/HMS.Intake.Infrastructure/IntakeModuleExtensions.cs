using Microsoft.Extensions.DependencyInjection;

namespace HMS.Intake.Infrastructure;

/// <summary>Intake module DI registration.</summary>
public static class IntakeModuleExtensions
{
    public static IServiceCollection AddIntakeModule(this IServiceCollection services)
    {
        // IIntakeDbContext registered in HMS.Persistence.PersistenceExtensions
        // FluentValidation registered in Program.cs via AddValidatorsFromAssembly
        return services;
    }
}
