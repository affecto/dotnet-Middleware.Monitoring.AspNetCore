using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Affecto.Middleware.Monitoring.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Monitoring.AspNetCore.Tests
{
    public class MonitoringMiddlewareTest
    {
        [Fact]
        public async void ShallowPathReturns204()
        {
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseMiddleware<MockMonitoringMiddleware>();
                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();
                HttpResponseMessage response = await client.GetAsync("/_monitor/shallow");
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Fact]
        public async void DeepPathReturns204()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(s => s.AddSingleton(GetHealthCheckServiceFactoryFor(SuccessHealthCheckAsync)))
                .Configure(app =>
                {
                    app.UseMiddleware<MockMonitoringMiddleware>();
                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                HttpResponseMessage response = await client.GetAsync("/_monitor/deep");
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                var msg = await response.Content.ReadAsStringAsync();
                Assert.Equal(string.Empty, msg);
            }
        }

        [Fact]
        public async void DeepPathReturns503()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(s => s.AddSingleton(GetHealthCheckServiceFactoryFor(FailureHealthCheckAsync)))
                .Configure(app =>
                {
                    app.UseMiddleware<MockMonitoringMiddleware>();
                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                HttpResponseMessage response = await client.GetAsync("/_monitor/deep");
                Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
                var msg = await response.Content.ReadAsStringAsync();
                Assert.NotEqual(string.Empty, msg);
            }
        }

        private static Func<IHealthCheckService> GetHealthCheckServiceFactoryFor(Func<Task> checkHealthAsync)
        {
            var mockHealthCheckService = Substitute.For<IHealthCheckService>();
            mockHealthCheckService.CheckHealthAsync().Returns(checkHealthAsync());
            Func<IHealthCheckService> HealthCheckServiceFactory = () => mockHealthCheckService;
            return HealthCheckServiceFactory;
        }

        private static Task SuccessHealthCheckAsync()
        {
            return Task.CompletedTask;
        }

        private static Task FailureHealthCheckAsync()
        {
            return Task.FromException(new Exception());
        }
    }

    internal class MockMonitoringMiddleware : MonitoringMiddleware
    {
        public MockMonitoringMiddleware(RequestDelegate next, string routePrefix = null, Func<IHealthCheckService> healthCheckServiceFactory = null)
            : base(next, routePrefix, healthCheckServiceFactory)
        {
        }
    }
}
