using HMS.Identity.Application.Abstractions;
using HMS.Identity.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HMS.Identity.Infrastructure.Authentication;

/// <summary>
/// JWT Service — generates access and refresh tokens.
///
/// CRITICAL FIX: Previously emitted tenant claim as "tenantId" but the entire
/// frontend auth layer, TenantProvider, and DashboardController all read "orgId".
/// Unified to "orgId" across the full stack.
///
/// Claim layout:
///   sub          → UserId
///   jti          → unique token ID
///   name         → FullName
///   orgId        → TenantId   ← FIXED (was "tenantId")
///   branchId     → BranchId
///   isGlobal     → "true" if Super Admin
///   role         → each Role name
///   permission   → each Permission name
/// </summary>
public sealed class JwtService(IConfiguration configuration) : IJwtService
{
    public string GenerateAccessToken(User user, List<string> roles, List<string> permissions)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier,   user.Id.ToString()),
            new(ClaimTypes.Name,             user.FullName),
        };

        // ── Tenant claim — unified to "orgId" ─────────────────────────────────
        if (user.TenantId != Guid.Empty)
            claims.Add(new Claim("orgId", user.TenantId.ToString()));
        else
            claims.Add(new Claim("isGlobal", "true"));

        // ── Branch claim ──────────────────────────────────────────────────────
        if (user.BranchId.HasValue)
            claims.Add(new Claim("branchId", user.BranchId.Value.ToString()));

        // ── Roles ─────────────────────────────────────────────────────────────
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        // ── Permissions ───────────────────────────────────────────────────────
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:            configuration["Jwt:Issuer"],
            audience:          configuration["Jwt:Audience"],
            claims:            claims,
            expires:           DateTime.UtcNow.AddMinutes(
                                   double.TryParse(configuration["Jwt:ExpiryMinutes"], out var m) ? m : 30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
