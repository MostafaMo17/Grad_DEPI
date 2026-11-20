using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Exceptions
{
    public class BadRequestException : Exception  //(400)
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }

    public class UnauthorizedException : Exception  //(401)
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }

    public class ForbiddenException : Exception   //(403)
    {
        public ForbiddenException(string message) : base(message)  
        {
        }
    }

    public class NotFoundException : Exception    //(404)
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }

    public class ConflictException : Exception   //(409)
    {
        public ConflictException(string message) : base(message)
        {
        }
    }

    public class LockedException : Exception   //(423)
    {
        public LockedException(string message) : base(message)
        {
        }
    }
}
