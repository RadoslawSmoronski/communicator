using System.Text.Json.Serialization;

namespace ChatCommunicator.Shared.Result
{
    public class Result
    {
        protected Result()
        {
            IsSuccess = true;
            Error = default;
        }

        protected Result(Error error)
        {
            IsSuccess = false;
            Error = error;
        }

        public bool IsSuccess { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Error? Error { get; }

        public static implicit operator Result(Error error) =>
            new(error);

        public static Result Success() =>
            new();

        public static Result Failure(Error error) =>
            new(error);
    }

}
