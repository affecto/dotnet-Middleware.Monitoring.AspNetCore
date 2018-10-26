using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Affecto.Middleware.Monitoring.AspNetCore
{
    public class DeepMonitoringMiddleware : MonitoringMiddleware
    {
        private readonly PathString monitorDeepPath;

        public DeepMonitoringMiddleware(RequestDelegate next, string routePrefix = null)
            : base(next, routePrefix)
        {
            monitorDeepPath = monitorPath.Value + "/deep";
        }

        public async Task Invoke(HttpContext context, Func<IHealthCheckService> healthCheckServiceFactory)
        {
            if (context.Request.Path.Value.EndsWith(monitorDeepPath))
            {
                await CheckHealth(context, healthCheckServiceFactory);
                return;
            }

            await next.Invoke(context);
        }

        private static async Task CheckHealth(HttpContext context, Func<IHealthCheckService> healthCheckServiceFactory)
        {
            if (healthCheckServiceFactory == null)
            {
                await ReturnErrorMessage(context, "No health check service factory available.");
                return;
            }

            IHealthCheckService healthCheckService;

            try
            {
                healthCheckService = healthCheckServiceFactory();
            }
            catch (Exception e)
            {
                await ReturnErrorMessage(context, $"Creating health check service failed: {e.Message}");
                return;
            }

            try
            {
                await healthCheckService.CheckHealthAsync().ConfigureAwait(false);
                context.Response.StatusCode = StatusCodes.Status204NoContent;
            }
            catch (Exception e)
            {
                string message;

                if (e is AggregateException && e.InnerException != null)
                {
                    message = e.InnerException.Message;
                }
                else
                {
                    message = e.Message;
                }

                await ReturnErrorMessage(context, message);
            }
        }

        private static async Task ReturnErrorMessage(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "text/plain";
            await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(message), 0, message.Length).ConfigureAwait(false);
        }
    }
}