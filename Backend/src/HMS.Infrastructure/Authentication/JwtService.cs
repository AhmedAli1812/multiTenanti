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
    // 🔑 Access Token (FIXED 🔥)
    // =========================
    public string GenerateToken(
    Guid userId,
    Guid? tenantId,
    List<string>? roles,
    List<string>? permissions,
    Guid? branchId,
    User user
)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Name, user.FullName ?? string.Empty)
    };

        // 🔥 BranchId
        if (branchId.HasValue)
        {
            claims.Add(new Claim("branchId", branchId.Value.ToString()));
        }

        // 🔥 Tenant
        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tenantId", tenantId.Value.ToString()));
        }
        else
        {
            claims.Add(new Claim("isGlobal", "true"));
        }

        // Roles
        if (roles != null && roles.Any())
        {
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        }

        // Permissions
        if (permissions != null && permissions.Any())
        {
            claims.AddRange(permissions.Select(p => new Claim("permission", p)));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}