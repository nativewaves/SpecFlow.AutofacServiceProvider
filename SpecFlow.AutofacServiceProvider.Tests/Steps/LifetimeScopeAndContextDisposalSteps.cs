using Autofac;
using BoDi;
using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;
using TechTalk.SpecFlow.Events;
using Xunit;

namespace NativeWaves.SpecFlow.AutofacServiceProvider.Tests.Steps
{
    [Binding, Scope(Feature = "LifetimeScopeAndContextDisposal")]
    public class LifetimeScopeAndContextDisposalSteps : IDisposable
    {
        private static ScenarioContext _context;
        private static IObjectContainer _featureContainer;
        private static IObjectContainer _scenarioContainer;

        public LifetimeScopeAndContextDisposalSteps(ScenarioContext context)
        {
            _context = context;
        }
        public void Dispose()
        {
            _featureContainer = null;
            _scenarioContainer = null;
        }

        [BeforeFeature]
        public static void FeatureSetup(
            IServiceProvider createAutofacServiceProvider,
            ITestThreadExecutionEventPublisher publisher,
            IObjectContainer c
            )
        {
            publisher.AddHandler<HookFinishedEvent>(CheckLifetimeScopeDisposed);
            _featureContainer = c;
            Assert.NotNull(c.Resolve<ILifetimeScope>());
            Assert.True(c.Resolve<ILifetimeScope>().Tag.ToString() == AutofacServiceProviderPlugin.FeatureScopeTag);
        }

        [BeforeScenario]
        public static void ScenarioSetup(
            IServiceProvider createAutofacServiceProvider,
            IObjectContainer c
            )
        {
            _scenarioContainer = c;
            Assert.NotNull(c.Resolve<ILifetimeScope>());
            Assert.True(c.Resolve<ILifetimeScope>().Tag.ToString() == AutofacServiceProviderPlugin.ScenarioScopeTag);
        }

        private static void CheckLifetimeScopeDisposed(HookFinishedEvent hookEvent)
        {
            if (_featureContainer == null || _scenarioContainer == null) { return; }

            IsDisposed(hookEvent, _featureContainer, HookType.AfterFeature);
            IsDisposed(hookEvent, _scenarioContainer, HookType.AfterScenario);

            void IsDisposed(HookFinishedEvent @event, IObjectContainer container, HookType hookType)
            {
                if (@event.HookType == hookType)
                {
                    var scope = container.Resolve<ILifetimeScope>();
                    Assert.Throws<ObjectDisposedException>(() => scope.Resolve<LifetimeScopeAndContextDisposalSteps>());
                }
            }
        }

        [Given(@"I have a context with number (.*)")]
        public void GivenIHaveAContextWithNumber(int number)
        {
            _context["number"] = number;
        }
    }
}
