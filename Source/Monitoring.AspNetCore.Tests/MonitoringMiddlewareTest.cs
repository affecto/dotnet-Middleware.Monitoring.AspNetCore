using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Affecto.Middleware.Monitoring.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Monitoring.AspNetCore.Tests
{
    public class MonitoringMiddlewareTest
    {
        private const string RoutePrefix = "SomeCustomRoute";

        [Fact]
        public async void ShallowAndDeepPathReturn204Together()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(s => s.AddSingleton(GetHealthCheckServiceFactoryFor(SuccessHealthCheckAsync)))
                .Configure(app => { app.UseMonitoring(); });

            using (TestServer server = new TestServer(builder))
            {
                HttpClient client = server.CreateClient();

                HttpResponseMessage response = await client.GetAsync("/_monitor/shallow");
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                response = await client.GetAsync("/_monitor/deep");
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                string message = await response.Content.ReadAsStringAsync();
                Assert.Equal(string.Empty, message);
            }
        }

        [Fact]
        public async void ShallowAndDeepPathUseRoutePrefixTogether()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(s => s.AddSingleton(GetHealthCheckServiceFactoryFor(SuccessHealthCheckAsync)))
                .Configure(app => { app.UseMonitoring(RoutePrefix); });

            using (TestServer server = new TestServer(builder))
            {
                HttpClient client = server.CreateClient();

                HttpResponseMessage response = await client.GetAsync($"{RoutePrefix}/_monitor/shallow");
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                response = await client.GetAsync($"{RoutePrefix}/_monitor/deep");
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                string message = await response.Content.ReadAsStringAsync();
                Assert.Equal(string.Empty, message);
            }
        }

        private static Func<IHealthCheckService> GetHealthCheckServiceFactoryFor(Func<Task> checkHealthAsync)
        {
            IHealthCheckService mockHealthCheckService = Substitute.For<IHealthCheckService>();
            mockHealthCheckService.CheckHealthAsync().Returns(checkHealthAsync());
            IHealthCheckService HealthCheckServiceFactory() => mockHealthCheckService;
            return HealthCheckServiceFactory;
        }

        private static Task SuccessHealthCheckAsync()
        {
            return Task.CompletedTask;
        }
    }
}
