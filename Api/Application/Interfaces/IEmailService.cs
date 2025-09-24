using Shared.Result;

namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task<Result> SendAsync(string to, string subject, string body);
    }
}
