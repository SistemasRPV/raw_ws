using Microsoft.Extensions.DependencyInjection;
using raw_ws.Repositories;

namespace raw_ws.Registers
{
    public static class CustomRegisters
    {
        /// <summary>
        /// Registro de los servicios/repositorios personalizados
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection addCustomRegisters(this IServiceCollection services)
        {
            services.AddTransient(typeof(MainRepository));
            return services;
        }
    }
}
