namespace ChatCommunicator.Shared.Result
{
    public class FailedResponseObject
    {
        public string Type { get; } = "https://tools.ietf.org/html/rfc9110#section-15.3.1";
        public string Title { get; set; } = String.Empty;
        public int Status { get; }
        public IDictionary<string, IEnumerable<string>> Errors { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public string? TraceId { get; set; }


        private Dictionary<int, string> _types = new Dictionary<int, string>
        {
            { 200, "OK" },
            { 201, "Created" },
            { 204, "No Content" },
            { 400, "Bad Request" },
            { 401, "Unauthorized" },
            { 403, "Forbidden" },
            { 404, "Not Found" },
            { 409, "Conflict" },
            { 422, "Unprocessable Entity" },
            { 500, "Internal Server Error" }
        };

    public FailedResponseObject(int code, string title, string errors)
        {
            Type = _types[code];
            Title = title;
        }
    }
}
