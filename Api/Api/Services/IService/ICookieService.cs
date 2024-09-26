namespace Api.Service.IService
{
    public interface ICookieService
    {
        void SetCookie(string name, string value, CookieOptions options);
    }
}
