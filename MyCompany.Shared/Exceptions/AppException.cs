using System;
using Microsoft.AspNetCore.Http; // 🔹 ajouté

namespace MyCompany.Shared.Exceptions
{
    public abstract class AppException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        protected AppException(string message, int statusCode, string errorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public class NotFoundException : AppException
        {
            public NotFoundException(string message)
                : base(message, StatusCodes.Status404NotFound, "NOT_FOUND") { }
        }

        public class ValidationException : AppException
        {
            public ValidationException(string message)
                : base(message, StatusCodes.Status400BadRequest, "VALIDATION_ERROR") { }
        }

        public class UnauthorizedException : AppException
        {
            public UnauthorizedException(string message = "Accès non autorisé")
                : base(message, StatusCodes.Status401Unauthorized, "UNAUTHORIZED") { }
        }

        public class ForbiddenException : AppException
        {
            public ForbiddenException(string message = "Accès interdit")
                : base(message, StatusCodes.Status403Forbidden, "FORBIDDEN") { }
        }

        public class ConflictException : AppException
        {
            public ConflictException(string message)
                : base(message, StatusCodes.Status409Conflict, "CONFLICT") { }
        }
    }
}
