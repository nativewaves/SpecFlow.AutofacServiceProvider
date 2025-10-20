using System;
using Reqnroll;

namespace NativeWaves.Reqnroll.AutofacServiceProvider
{
    [Serializable]
    public class MissingScenarioDependenciesException : ReqnrollException
    {
        public MissingScenarioDependenciesException()
            : base("No method marked with [ScenarioDependencies] attribute found.")
        {
            HelpLink = @"https://github.com/solidtoken/Reqnroll.DependencyInjection#usage";
        }
    }
}
