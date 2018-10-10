using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Affecto.Middleware.Monitoring.AspNetCore
{
    public class MonitoringMiddleware
    {
        private readonly RequestDelegate next;
        private readonly Func<IHealthCheckService> healthCheckServiceFactory;
        private readonly PathString monitorPath;
        private readonly PathString monitorShallowPath;
        private readonly PathString monitorDeepPath;

        public MonitoringMiddleware(RequestDelegate next, string routePrefix = null, Func<IHealthCheckService> healthCheckServiceFactory = null)
        {
            if (string.IsNullOrWhiteSpace(routePrefix))
            {
                routePrefix = string.Empty;
            }
            else if (!routePrefix.StartsWith("/"))
            {
                routePrefix = "/" + routePrefix;
            }

            monitorPath = new PathString(routePrefix + "/_monitor");
            monitorShallowPath = new PathString(routePrefix + "/_monitor/shallow");
            monitorDeepPath = new PathString(routePrefix + "/_monitor/deep");

            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.healthCheckServiceFactory = healthCheckServiceFactory;
        }

        public Task Invoke(HttpContext context)
        {
            if (context.Request.Path.ToString().Contains(monitorPath))
            {
                return HandleMonitorEndpoint(context);
            }

            return next.Invoke(context);
        }

        private Task HandleMonitorEndpoint(HttpContext context)
        {
            if (context.Request.Path.ToString().Contains(monitorShallowPath))
            {
                return ShallowEndpoint(context);
            }
            if (context.Request.Path.ToString().Contains(monitorDeepPath))
            {
                return DeepEndpoint(context);
            }

            return Task.CompletedTask;
        }

        private async Task DeepEndpoint(HttpContext context)
        {
            if (healthCheckServiceFactory == null)
            {
                context.Response.StatusCode = StatusCodes.Status204NoContent;
            }
            else
            {
                try
                {
                    await healthCheckServiceFactory().CheckHealthAsync().ConfigureAwait(false);
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

                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    context.Response.ContentType = "text/plain";
                    await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(message), 0, message.Length);
                }
            }
        }

        private static Task ShallowEndpoint(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        }
    }
}