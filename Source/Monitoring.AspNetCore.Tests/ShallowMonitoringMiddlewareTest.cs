using System.Net;
using System.Net.Http;
using Affecto.Middleware.Monitoring.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Monitoring.AspNetCore.Tests
{
    public class ShallowMonitoringMiddlewareTest
    {
        private const string RoutePrefix = "SomeCustomRoute";

        [Fact]
        public async void ShallowPathReturns204()
        {
            var builder = new WebHostBuilder()
                .Configure(app => { app.UseShallowMonitoring(); });

            using (TestServer server = new TestServer(builder))
            {
                HttpClient client = server.CreateClient();
                HttpResponseMessage response = await client.GetAsync("/_monitor/shallow");
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Fact]
        public async void ShallowPathUsesRoutePrefix()
        {
            var builder = new WebHostBuilder()
                .Configure(app => { app.UseShallowMonitoring(RoutePrefix); });

            using (TestServer server = new TestServer(builder))
            {
                HttpClient client = server.CreateClient();
                HttpResponseMessage response = await client.GetAsync($"{RoutePrefix}/_monitor/shallow");
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
    }
}
