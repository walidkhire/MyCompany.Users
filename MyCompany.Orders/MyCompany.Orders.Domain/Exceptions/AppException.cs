using System;
using Microsoft.AspNetCore.Http;

namespace MyCompany.Orders.Domain.Exceptions
{
    // 1️⃣ La classe de base abstraite
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
    }

    // 2️⃣ Les classes dérivées (Désimbriquées, au même niveau dans le namespace)
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

    // 🔹 AJOUT : Vous pouvez aussi ajouter explicitement BadRequestException si vous préférez ce nom !
    public class BadRequestException : AppException
    {
        public BadRequestException(string message)
            : base(message, StatusCodes.Status400BadRequest, "BAD_REQUEST") { }
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