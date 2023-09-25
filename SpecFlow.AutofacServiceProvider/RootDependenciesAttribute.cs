using System;

namespace NativeWaves.SpecFlow.AutofacServiceProvider
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RootDependenciesAttribute : Attribute
    {
        /// <summary>
        /// Automatically register all SpecFlow bindings.
        /// </summary>
        public bool AutoRegisterBindings { get; set; } = true;
    }
}
