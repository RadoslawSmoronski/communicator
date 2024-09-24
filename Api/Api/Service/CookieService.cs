
namespace Api.Service
{
    public class CookieService : ICookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetCookie(string name, string value, CookieOptions options)
        {
            var response = _httpContextAccessor.HttpContext.Response;
            response.Cookies.Append(name, value, options);
        }
    }
}
