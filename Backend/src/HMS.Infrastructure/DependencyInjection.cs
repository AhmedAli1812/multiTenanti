// ──────────────────────────────────────────────────────────────────────────
// THIS FILE IS INTENTIONALLY EMPTY / DEPRECATED.
//
// FIX (Bug #11): TenantProvider was registered twice:
//   • Here (Infrastructure root DependencyInjection.cs)
//   • In HMS.Infrastructure.DependencyInjection.ServiceCollectionExtensions
//
// The second registration silently overrides the first, making this file's
// registrations dead code. The correct, canonical registration point is:
//
//   HMS.Infrastructure.DependencyInjection.ServiceCollectionExtensions
//
// All registrations have been consolidated there. This file is kept as a
// placeholder to avoid breaking project references, but contains no logic.
// ──────────────────────────────────────────────────────────────────────────

namespace HMS.Infrastructure.DependencyInjection;

// Empty — see ServiceCollectionExtensions.cs