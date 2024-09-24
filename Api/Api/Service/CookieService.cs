
namespace Api.Service
{
    public class CookieService : ICookieService
    {
        private readonly HttpResponse _response;

        public CookieService(HttpResponse response)
        {
            _response = response;
        }

        public void SetCookie(string name, string value, CookieOptions options)
        {
            _response.Cookies.Append(name, value, options);
        }
    }
}
