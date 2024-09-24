namespace Api.Service
{
    public interface ICookieService
    {
        void SetCookie(string name, string value, CookieOptions options);
    }
}
