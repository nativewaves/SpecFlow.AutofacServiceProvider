using System;

namespace NativeWaves.Reqnroll.AutofacServiceProvider
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RootDependenciesAttribute : Attribute
    {
        /// <summary>
        /// Automatically register all Reqnroll bindings.
        /// </summary>
        public bool AutoRegisterBindings { get; set; } = true;
    }
}
