using Microsoft.AspNetCore.Builder;

namespace Affecto.Middleware.Monitoring.AspNetCore
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMonitoring(this IApplicationBuilder builder)
        {
            return builder
                .UseShallowMonitoring()
                .UseDeepMonitoring();
        }

        public static IApplicationBuilder UseMonitoring(this IApplicationBuilder builder, string routePrefix)
        {
            return builder
                .UseShallowMonitoring(routePrefix)
                .UseDeepMonitoring(routePrefix);
        }

        public static IApplicationBuilder UseShallowMonitoring(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ShallowMonitoringMiddleware>();
        }

        public static IApplicationBuilder UseShallowMonitoring(this IApplicationBuilder builder, string routePrefix)
        {
            return builder.UseMiddleware<ShallowMonitoringMiddleware>(routePrefix);
        }

        public static IApplicationBuilder UseDeepMonitoring(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DeepMonitoringMiddleware>();
        }

        public static IApplicationBuilder UseDeepMonitoring(this IApplicationBuilder builder, string routePrefix)
        {
            return builder.UseMiddleware<DeepMonitoringMiddleware>(routePrefix);
        }

        public static IApplicationBuilder UseMonitoring<TMiddleware>(this IApplicationBuilder builder) where TMiddleware : MonitoringMiddleware
        {
            return builder.UseMiddleware<TMiddleware>();
        }

        public static IApplicationBuilder UseMonitoring<TMiddleware>(this IApplicationBuilder builder, string routePrefix) where TMiddleware : MonitoringMiddleware
        {
            return builder.UseMiddleware<TMiddleware>(routePrefix);
        }
    }
}