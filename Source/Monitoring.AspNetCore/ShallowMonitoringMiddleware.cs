using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Affecto.Middleware.Monitoring.AspNetCore
{
    public class ShallowMonitoringMiddleware : MonitoringMiddleware
    {
        private readonly PathString monitorShallowPath;

        public ShallowMonitoringMiddleware(RequestDelegate next, string routePrefix = null)
            : base(next, routePrefix)
        {
            monitorShallowPath = monitorPath.Value + "/shallow";
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value.EndsWith(monitorShallowPath) || context.Request.Path.Value.EndsWith(monitorPath))
            {
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            await next.Invoke(context);
        }
    }
}