using HMS.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;



namespace HMS.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return userId != null ? Guid.Parse(userId) : Guid.Empty;
            }
        }
        public Guid TenantId =>
    Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst("tenantId")!.Value);
    }
}