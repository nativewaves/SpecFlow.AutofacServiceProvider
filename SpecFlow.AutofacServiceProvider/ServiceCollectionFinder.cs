using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;

namespace NativeWaves.SpecFlow.AutofacServiceProvider
{
    public class ServiceCollectionFinder : IServiceCollectionFinder
    {
        private readonly IBindingRegistry bindingRegistry;
        private IServiceCollection _cache;

        public ServiceCollectionFinder(IBindingRegistry bindingRegistry)
        {
            this.bindingRegistry = bindingRegistry;
        }

        public IServiceCollection GetServiceCollection()
        {
            if (_cache != default)
            {
                return _cache;
            }

            var assemblies = bindingRegistry.GetBindingAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        var scenarioDependenciesAttribute = (RootDependenciesAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(RootDependenciesAttribute));

                        if (scenarioDependenciesAttribute != null)
                        {
                            var serviceCollection = GetServiceCollection(methodInfo);
                            if (scenarioDependenciesAttribute.AutoRegisterBindings)
                            {
                                AddBindingAttributes(assemblies, serviceCollection);
                            }
                            return _cache = serviceCollection;
                        }
                    }
                }
            }
            throw new MissingScenarioDependenciesException();
        }

        private static IServiceCollection GetServiceCollection(MethodBase methodInfo)
        {
            return (IServiceCollection)methodInfo.Invoke(null, null);
        }

        private static void AddBindingAttributes(IEnumerable<Assembly> bindingAssemblies, IServiceCollection serviceCollection)
        {
            foreach (var assembly in bindingAssemblies)
            {
                foreach (var type in assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(BindingAttribute))))
                {
                    serviceCollection.AddScoped(type);
                }
            }
        }
    }
}
