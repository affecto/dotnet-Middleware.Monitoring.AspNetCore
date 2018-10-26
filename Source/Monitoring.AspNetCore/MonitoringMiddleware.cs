using System;
using Microsoft.AspNetCore.Http;

namespace Affecto.Middleware.Monitoring.AspNetCore
{
    public abstract class MonitoringMiddleware
    {
        protected readonly RequestDelegate next;
        protected readonly PathString monitorPath;

        protected MonitoringMiddleware(RequestDelegate next, string routePrefix = null)
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

            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }
    }
}