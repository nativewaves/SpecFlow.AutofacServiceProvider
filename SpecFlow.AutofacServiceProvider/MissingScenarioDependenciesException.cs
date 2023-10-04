using System;
using TechTalk.SpecFlow;

namespace NativeWaves.SpecFlow.AutofacServiceProvider
{
    [Serializable]
    public class MissingScenarioDependenciesException : SpecFlowException
    {
        public MissingScenarioDependenciesException()
            : base("No method marked with [ScenarioDependencies] attribute found.")
        {
            HelpLink = @"https://github.com/solidtoken/SpecFlow.DependencyInjection#usage";
        }
    }
}
