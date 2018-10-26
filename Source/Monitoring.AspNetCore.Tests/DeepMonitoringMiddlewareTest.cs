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
    public class DeepMonitoringMiddlewareTest
    {
        private const string RoutePrefix = "SomeCustomRoute";
        private const string ErrorMessage = "This is an error from health check.";

        [Fact]
        public async void DeepPathReturns204()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(s => s.AddSingleton(GetHealthCheckServiceFactoryFor(SuccessHealthCheckAsync)))
                .Configure(app => { app.UseDeepMonitoring(); });

            using (TestServer server = new TestServer(builder))
            {
                HttpClient client = server.CreateClient();

                HttpResponseMessage response = await client.GetAsync("/_monitor/deep");
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                string message = await response.Content.ReadAsStringAsync();
                Assert.Equal(string.Empty, message);
            }
        }

        [Fact]
        public async void DeepPathReturns503()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(s => s.AddSingleton(GetHealthCheckServiceFactoryFor(FailureHealthCheckAsync)))
                .Configure(app => { app.UseDeepMonitoring(); });

            using (TestServer server = new TestServer(builder))
            {
                HttpClient client = server.CreateClient();

                HttpResponseMessage response = await client.GetAsync("/_monitor/deep");
                Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

                string message = await response.Content.ReadAsStringAsync();
                Assert.Equal(ErrorMessage, message);
            }
        }

        [Fact]
        public async void DeepPathReturnsUsesRoutePrefix()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(s => s.AddSingleton(GetHealthCheckServiceFactoryFor(SuccessHealthCheckAsync)))
                .Configure(app => { app.UseDeepMonitoring(RoutePrefix); });

            using (TestServer server = new TestServer(builder))
            {
                HttpClient client = server.CreateClient();

                HttpResponseMessage response = await client.GetAsync($"{RoutePrefix}/_monitor/deep");
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                string message = await response.Content.ReadAsStringAsync();
                Assert.Equal(string.Empty, message);
            }
        }

        [Fact]
        public async void DeepPathUnwrapsAggregateExceptions()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(s => s.AddSingleton(GetHealthCheckServiceFactoryFor(FailureHealthCheckWithAggregateExceptionAsync)))
                .Configure(app => { app.UseDeepMonitoring(); });

            using (TestServer server = new TestServer(builder))
            {
                HttpClient client = server.CreateClient();

                HttpResponseMessage response = await client.GetAsync("/_monitor/deep");
                Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

                string message = await response.Content.ReadAsStringAsync();
                Assert.Equal(ErrorMessage, message);
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

        private static Task FailureHealthCheckAsync()
        {
            return Task.FromException(new Exception(ErrorMessage));
        }

        private static Task FailureHealthCheckWithAggregateExceptionAsync()
        {
            return Task.FromException(new AggregateException("This comes from the aggregate exception.", new Exception(ErrorMessage)));
        }
    }
}
