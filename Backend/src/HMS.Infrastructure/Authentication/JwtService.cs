using HMS.Application.Abstractions.Auth;
using HMS.Domain.Entities.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // =========================
    // 🔐 Refresh Token
    // =========================
    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    // =========================
    // 🔑 Access Token
    // =========================
    public string GenerateToken(
        Guid userId,
        Guid? tenantId,
        List<string>? roles,
        List<string>? permissions,
        User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // =========================
        // 🔥 Claims الأساسية
        // =========================
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName ?? string.Empty)
        };

        // =========================
        // 🏢 Tenant (FIXED 🔥)
        // =========================
        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tenantId", tenantId.Value.ToString())); // ✅ FIX
        }
        else
        {
            claims.Add(new Claim("isGlobal", "true"));
        }

        // =========================
        // 👤 Roles
        // =========================
        if (roles != null && roles.Any())
        {
            claims.AddRange(
                roles.Distinct()
                     .Select(role => new Claim(ClaimTypes.Role, role))
            );
        }

        // =========================
        // 🔐 Permissions
        // =========================
        if (permissions != null && permissions.Any())
        {
            claims.AddRange(
                permissions.Distinct()
                           .Select(p => new Claim("permission", p))
            );
        }

        // =========================
        // 🔐 Signing Key
        // =========================
        var keyValue = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(keyValue))
            throw new InvalidOperationException("Jwt:Key is not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // =========================
        // ⏱ Expiration
        // =========================
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var mins)
            ? mins
            : 30;

        // =========================
        // 🧾 Token
        // =========================
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}