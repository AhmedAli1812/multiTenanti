namespace HMS.Application.Abstractions.Caching;

public interface IPermissionCacheService
{
    Task RemoveAsync(Guid userId);

    Task RemoveRoleAsync(Guid roleId);

    Task<List<string>> GetAllPermissionsAsync(Guid userId);

    Task SetAllPermissionsAsync(Guid userId, List<string> permissions);
}