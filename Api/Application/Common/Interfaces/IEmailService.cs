using Shared.Result;

namespace Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task<Result> SendAsync(string to, string subject, string body);
    }
}
