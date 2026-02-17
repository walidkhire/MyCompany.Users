namespace MyCompany.Orders.Domain.Exceptions
{
    public abstract class AppException : Exception
    {
        public int StatusCode { get; }
        protected AppException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string msg) : base(msg, 404) { }
    }

    public class BadRequestException : AppException
    {
        public BadRequestException(string msg) : base(msg, 400) { }
    }
}
