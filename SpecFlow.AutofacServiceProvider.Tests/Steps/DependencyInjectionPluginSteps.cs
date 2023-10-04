using TechTalk.SpecFlow;
using Xunit;

namespace NativeWaves.SpecFlow.AutofacServiceProvider.Tests.Steps
{
    [Binding]
    public class DependencyInjectionPluginSteps
    {
        private readonly ITestService _testService;

        public DependencyInjectionPluginSteps(ITestService testService)
        {
            _testService = testService;
        }

        [Then(@"verify that TestService is correctly injected")]
        public void ThenVerifyThatTestServiceIsCorrectlyInjected()
        {
            Assert.True(_testService.Verify());
        }
    }

    public interface ITestService
    {
        bool Verify();
    }

    public class TestService : ITestService
    {
        public bool Verify()
        {
            return true;
        }
    }

    [Binding, Scope(Feature = "DependencyInjectionPlugin")]
    public class HookCheck
    {
        [BeforeFeature]
        public static void FeatureCheck(FeatureContext f, IFeatureContext f2)
        {
            Assert.NotNull(f);
            Assert.NotNull(f2);
        }

        [BeforeScenario]
        public static void ScenarioCheck(ScenarioContext s, IScenarioContext s2)
        {
            Assert.NotNull(s);
            Assert.NotNull(s2);
        }
    }

    [Binding]
    public class IFeatureContextSteps
    {
        private readonly IFeatureContext _context;
        public IFeatureContextSteps(IFeatureContext context) => _context = context;
        [Then(@"the IFeatureContext is correctly injected")]
        public void ThenTheFeatureContextIsCorrectlyInjected() => Assert.NotNull(_context);
    }

    [Binding]
    public class FeatureContextSteps
    {
        private readonly FeatureContext _context;
        public FeatureContextSteps(FeatureContext context) => _context = context;
        [Then(@"the FeatureContext is correctly injected")]
        public void ThenTheFeatureContextIsCorrectlyInjected() => Assert.NotNull(_context);
    }

    [Binding]
    public class IScenarioContextSteps
    {
        private readonly IScenarioContext _context;
        public IScenarioContextSteps(IScenarioContext context) => _context = context;
        [Then(@"the IScenarioContext is correctly injected")]
        public void ThenTheIScenarioContextIsCorrectlyInjected() => Assert.NotNull(_context);
    }

    [Binding]
    public class ScenarioContextSteps
    {
        private readonly ScenarioContext _context;
        public ScenarioContextSteps(ScenarioContext context) => _context = context;
        [Then(@"the ScenarioContext is correctly injected")]
        public void ThenTheScenarioContextIsCorrectlyInjected() => Assert.NotNull(_context);
    }
}
