using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using TechTalk.SpecFlow;
using Xunit;

namespace NativeWaves.SpecFlow.AutofacServiceProvider.Tests.Steps
{
    public interface ITestContext
    {
        int Number { get; set; }
    }
    public class TestContext : ITestContext
    {
        public int Number { get; set; }
    }

    [Binding, Scope(Tag = "ScenarioRegistration"), Scope(Tag = "FeatureRegistration")]
    public class ContextInjectionScopeSteps
    {
        private readonly ITestContext _context;

        public ContextInjectionScopeSteps(ITestContext context)
        {
            _context = context;
        }

        [BeforeFeature]
        public static void SetupViaFeature(IServiceCollection services, FeatureContext context)
        {
            if (context.FeatureInfo.Tags.Contains("FeatureRegistration"))
            {
                services.AddScoped<ITestContext, TestContext>();
            }
        }

        [BeforeScenario]
        public static void SetupViaScenario(IServiceCollection services, ScenarioContext context)
        {
            if (context.ScenarioInfo.Tags.Contains("ScenarioRegistration"))
            {
                // inject an instance which is the same for all injections in one scenario
                services.AddScoped<ITestContext, TestContext>();
            }
        }

        [Given(@"I have a test context")]
        public void GivenIHaveATestContext()
        {
            // NOOP
        }

        [Given(@"I have test context with number (.*)")]
        public void GivenIHaveTestContextWithNumber(int number)
        {
            _context.Number = number;
        }

        [Then(@"the test context number should be (.*)")]
        public void ThenTheTestContextNumberShouldBe(int expected)
        {
            Assert.Equal(expected, _context.Number);
        }
    }

    [Binding, Scope(Tag = "ScenarioRegistration"), Scope(Tag = "FeatureRegistration")]
    public class MultiplySteps
    {
        private readonly ITestContext _context;

        public MultiplySteps(ITestContext context)
        {
            _context = context;
        }

        [When(@"I multiply the test context number by (.*)")]
        public void WhenIMultiplyTheTestContextNumberBy(int multiply)
        {
            _context.Number *= multiply;
        }
    }

    [Binding, Scope(Tag = "ScenarioRegistration"), Scope(Tag = "FeatureRegistration")]
    public class IncreaseSteps
    {
        private readonly ITestContext _context;

        public IncreaseSteps(ITestContext context)
        {
            _context = context;
        }

        [When(@"I increase the test context number by (.*)")]
        public void WhenIIncreaseTheTestContextNumberBy(int increase)
        {
            _context.Number += increase;
        }
    }
}
