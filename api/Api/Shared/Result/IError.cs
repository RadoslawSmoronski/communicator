namespace Shared.Result
{
    public interface IError<T>
    {
        public string Code { get; }
        public string Description { get; }
        public T ErrorType { get; }


    }
}
