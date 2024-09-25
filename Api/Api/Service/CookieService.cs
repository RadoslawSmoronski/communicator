using Api.Service;
using Microsoft.AspNetCore.Http;

public class CookieService : ICookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetCookie(string name, string value, CookieOptions options)
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        if (response != null)
        {
            response.Cookies.Append(name, value, options);
        }
    }
}