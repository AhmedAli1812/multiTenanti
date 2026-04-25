using HMS.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace HMS.Infrastructure.Caching;

public class PermissionCacheService : IPermissionCacheService
{
    private readonly IMemoryCache _cache;

    public PermissionCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    // =========================
    // 🔥 Get Permissions
    // =========================
    public Task<List<string>> GetAllPermissionsAsync(Guid userId)
    {
        _cache.TryGetValue(userId, out List<string>? permissions);
        return Task.FromResult(permissions ?? new List<string>());
    }

    // =========================
    // 🔥 Set Permissions
    // =========================
    public Task SetAllPermissionsAsync(Guid userId, List<string> permissions)
    {
        _cache.Set(userId, permissions, TimeSpan.FromMinutes(30));
        return Task.CompletedTask;
    }

    // =========================
    // 🔥 Remove User Cache
    // =========================
    public Task RemoveAsync(Guid userId)
    {
        _cache.Remove(userId);
        return Task.CompletedTask;
    }

    // =========================
    // 🔥 Remove Role Cache (invalidate all)
    // =========================
    public Task RemoveRoleAsync(Guid roleId)
    {
        // 💣 بسيط دلوقتي: امسح الكاش كله
        // بعدين ممكن تعمل tracking per role
        if (_cache is MemoryCache memCache)
        {
            memCache.Compact(1.0);
        }

        return Task.CompletedTask;
    }
}