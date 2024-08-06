using Api.Models;

namespace Api.Service
{
    public interface ITokenService
    {         
        string CreateToken(UserAccount user);
    }
}
