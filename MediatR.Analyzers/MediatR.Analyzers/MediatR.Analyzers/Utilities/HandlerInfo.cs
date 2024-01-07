namespace MediatR.Analyzers.Utilities
{
    public class HandlerInfo
    {
        public HandlerInfo(string handler, string request)
        {
            Handler = handler;
            Request = request;
        }

        public string Handler { get; }
        public string Request { get; }
    }
}
