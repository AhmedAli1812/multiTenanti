using HMS.Application.Abstractions.Security;
using Microsoft.AspNetCore.Http;

public class RequestInfoProvider : IRequestInfoProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestInfoProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    public string GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }
}