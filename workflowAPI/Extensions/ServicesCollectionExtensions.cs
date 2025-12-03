using Polly;
using Polly.Extensions.Http;
using Serilog;
using workflowAPI.Configuration;
using workflowAPI.Services;

namespace workflowAPI.Extensions
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddWorkflowServices(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            // Configure WorkflowConfiguration
            services.Configure<WorkflowConfiguration>(
                configuration.GetSection("WorkflowConfiguration"));

            // Register Scheme Registry (Singleton - load once)
            services.AddSingleton<IWorkflowSchemeRegistry, WorkflowSchemeRegistry>();

            // Register HTTP Client with Polly policies
            services.AddHttpClient<IWorkflowService, WorkflowService>(client =>
            {
                var baseUrl = configuration["WorkflowServer:BaseUrl"] ?? "http://localhost:8077";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(
                    configuration.GetValue<int>("WorkflowServer:Timeout", 30));
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            return services;
        }


        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Log.Warning(
                            "Retry {RetryAttempt} after {Delay}ms due to {Exception}",
                            retryAttempt,
                            timespan.TotalMilliseconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, duration) =>
                    {
                        Log.Error("Circuit breaker opened for {Duration}s", duration.TotalSeconds);
                    },
                    onReset: () =>
                    {
                        Log.Information("Circuit breaker reset");
                    });
        }
    }
}
