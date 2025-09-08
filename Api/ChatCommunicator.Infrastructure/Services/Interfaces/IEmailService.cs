using ChatCommunicator.Shared.Result;

namespace ChatCommunicator.Infrastructure.Services.Interfaces
{
    public interface IEmailService
    {
        Task<Result> SendAsync(string to, string subject, string body); 
    }
}
