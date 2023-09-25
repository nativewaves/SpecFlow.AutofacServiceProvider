using Microsoft.Extensions.DependencyInjection;
using NativeWaves.SpecFlow.AutofacServiceProvider.Tests.Steps;

namespace NativeWaves.SpecFlow.AutofacServiceProvider.Tests.Support
{
    public static class Dependencies
    {
        [RootDependencies]
        public static IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();

            // Add test dependencies
            services.AddTransient<ITestService, TestService>();

            // ContextInjectionScope (by using AddScoped instead of AddTransient, the context will be scoped to the Feature across bindings)
            services.AddScoped<TestContext>();

            // Calculator
            services.AddScoped<ICalculator, Calculator>();

            return services;
        }
    }
}
