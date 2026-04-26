using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace HMS.API.Filters
{
    public class AuthorizePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permission;

        public AuthorizePermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // ❌ مش عامل login
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // 🔥 1. اقرأ permissions من التوكن (الأسرع)
            var permissions = user.Claims
    .Where(c => c.Type == "permission")
    .Select(c => c.Value);

            if (!permissions.Contains(_permission))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}