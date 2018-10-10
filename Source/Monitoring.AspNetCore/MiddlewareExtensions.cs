using Microsoft.AspNetCore.Builder;

namespace Affecto.Middleware.Monitoring.AspNetCore
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMonitoring(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MonitoringMiddleware>();
        }

        public static IApplicationBuilder UseMonitoring<TMiddleware>(this IApplicationBuilder builder) where TMiddleware : MonitoringMiddleware
        {
            return builder.UseMiddleware<TMiddleware>();
        }
    }
}