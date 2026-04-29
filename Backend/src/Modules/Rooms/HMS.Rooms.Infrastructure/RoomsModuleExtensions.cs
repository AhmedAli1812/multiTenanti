using Microsoft.Extensions.DependencyInjection;

namespace HMS.Rooms.Infrastructure;

/// <summary>Rooms module DI registration.</summary>
public static class RoomsModuleExtensions
{
    public static IServiceCollection AddRoomsModule(this IServiceCollection services)
    {
        // IRoomsDbContext registered in HMS.Persistence.PersistenceExtensions
        // FluentValidation registered in Program.cs via AddValidatorsFromAssembly
        return services;
    }
}
