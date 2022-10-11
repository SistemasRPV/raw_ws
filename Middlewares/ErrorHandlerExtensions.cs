using Microsoft.AspNetCore.Builder;

namespace raw_ws.Middlewares
{
    /// <summary>
    /// Clase para la creación del middleware reutilizable
    /// </summary>
    public static class ErrorHandlerExtensions
    {        
        public static IApplicationBuilder UseErrorHandlerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
