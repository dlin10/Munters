using System;

namespace Munters.ResourceAccess
{
    public class ApiException : Exception
    {
        public ApiException(string message, int statusCode, string response, Exception inner = null) : base(message, inner)
        {
            StatusCode = statusCode;
            Response = response;
        }
        public int StatusCode { get; }

        public string Response { get; }
    }
}