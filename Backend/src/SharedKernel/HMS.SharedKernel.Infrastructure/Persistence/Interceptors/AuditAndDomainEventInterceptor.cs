using HMS.SharedKernel.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;


namespace HMS.SharedKernel.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that:
///  1. Auto-fills CreatedAt/UpdatedAt on SaveChanges (replaces inline audit in DbContext)
///  2. Dispatches domain events collected by entities AFTER the transaction commits
///
/// By dispatching AFTER commit, we guarantee that event handlers see committed data —
/// never stale/uncommitted state.
/// </summary>
public sealed class AuditAndDomainEventInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    // ── Before save: stamp audit fields ──────────────────────────────────────
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null) return new(result);

        var now    = DateTime.UtcNow;
        var userId = ResolveCurrentUserId(eventData.Context);

        foreach (var entry in eventData.Context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }

        return new(result);
    }

    // ── After save: dispatch domain events ────────────────────────────────────
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null) return result;

        // Collect all domain events before clearing them
        var domainEvents = eventData.Context.ChangeTracker
            .Entries<BaseEntity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // Clear first — re-entrancy guard
        foreach (var entry in eventData.Context.ChangeTracker.Entries<BaseEntity>())
            entry.Entity.ClearDomainEvents();

        // Dispatch. Each event handler runs in its own scoped transaction if needed.
        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent, cancellationToken);

        return result;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static Guid? ResolveCurrentUserId(DbContext context)
    {
        // Pull from service locator pattern only as fallback.
        // Prefer injecting ICurrentUser directly where possible.
        try
        {
            var currentUser = context.GetService<ICurrentUser>();
            return currentUser?.UserId;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Minimal interface so the interceptor can resolve UserId without
/// a hard dependency on the full CurrentUser implementation.
/// </summary>
public interface ICurrentUser
{
    Guid?   UserId   { get; }
    Guid?   TenantId { get; }
    bool    IsGlobal { get; }
}
