using System;

namespace raw_ws.Helpers
{
    /// <summary>
    /// Excepciones personalizadas utilizadas en el middleware de excepciones
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string msg) : base(msg) { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string msg) : base(msg) { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string msg) : base(msg) { }
    }

    public class NotAlloedException : Exception
    {
        public NotAlloedException(string msg) : base(msg) { }
    }

    public class UnprocessableEntityException : Exception
    {
        public UnprocessableEntityException(string msg) : base(msg) { }
    }

    ///Excepciones personalizadas
    public class NotContentException : Exception
    {
        public NotContentException(string msg) : base(msg) { }
    }

    public class SqlException : Exception
    {
        public SqlException(string msg) : base(msg) { }
    }
}
