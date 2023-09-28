using Autofac;
using Autofac.Extensions.DependencyInjection;
using BoDi;
using Microsoft.Extensions.DependencyInjection;
using NativeWaves.SpecFlow.AutofacServiceProvider;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;
using TechTalk.SpecFlow.Bindings.Discovery;
using TechTalk.SpecFlow.BindingSkeletons;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.ErrorHandling;
using TechTalk.SpecFlow.Events;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.Plugins;
using TechTalk.SpecFlow.Tracing;
using TechTalk.SpecFlow.UnitTestProvider;

[assembly: RuntimePlugin(typeof(AutofacServiceProviderPlugin))]

namespace NativeWaves.SpecFlow.AutofacServiceProvider
{
    public class AutofacServiceProviderPlugin : IRuntimePlugin
    {
        public const string FeatureScopeTag = "Feature-Scope";
        public const string ScenarioScopeTag = "Scenario-Scope";
        private static readonly ConcurrentDictionary<ILifetimeScope, IContextManager> BindMappings = new ConcurrentDictionary<ILifetimeScope, IContextManager>();
        private static readonly ConcurrentDictionary<ISpecFlowContext, ILifetimeScope> ActiveServiceScopes = new ConcurrentDictionary<ISpecFlowContext, ILifetimeScope>();
        private readonly object _registrationLock = new object();

        public void Initialize(
            RuntimePluginEvents runtimePluginEvents,
            RuntimePluginParameters runtimePluginParameters,
            UnitTestProviderConfiguration unitTestProviderConfiguration
            )
        {
            runtimePluginEvents.CustomizeGlobalDependencies += CustomizeGlobalDependencies;
            runtimePluginEvents.CustomizeFeatureDependencies += CustomizeFeatureDependenciesEventHandler;
            runtimePluginEvents.CustomizeScenarioDependencies += CustomizeScenarioDependenciesEventHandler;
        }

        private void CustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs args)
        {
            if (!args.ObjectContainer.IsRegistered<IServiceCollectionFinder>())
            {
                lock (_registrationLock)
                {
                    if (!args.ObjectContainer.IsRegistered<IServiceCollectionFinder>())
                    {
                        args.ObjectContainer.RegisterTypeAs<DependencyInjectionTestObjectResolver, ITestObjectResolver>();
                        args.ObjectContainer.RegisterTypeAs<ServiceCollectionFinder, IServiceCollectionFinder>();
                    }
                    
                    // We store the (MS) service provider in the global (BoDi) container, we create it only once.
                    // It must be lazy (hence factory) because at this point we still don't have the bindings mapped.
                    args.ObjectContainer.RegisterFactoryAs(c =>
                    {
                        var serviceCollectionFinder = c.Resolve<IServiceCollectionFinder>();
                        var services = serviceCollectionFinder.GetServiceCollection();

                        RegisterProxyBindings(c, services);
                        c.RegisterInstanceAs(services);
                        var builder = new Autofac.ContainerBuilder();
                        builder.Populate(services);
                        PrintRegisteredServiceInfo(c);
                        var autofacContainer = builder.Build();
                        c.RegisterInstanceAs<ILifetimeScope>(autofacContainer);
                        return autofacContainer;
                    });

                    args.ObjectContainer.RegisterFactoryAs<IServiceProvider>(c =>
                    {
                        return new Autofac.Extensions.DependencyInjection.AutofacServiceProvider(c.Resolve<IContainer>());
                    });

                    // Will make sure the scope is disposed.
                    var lcEvents = args.ObjectContainer.Resolve<RuntimePluginTestExecutionLifecycleEvents>();
                    lcEvents.AfterScenario += AfterScenarioPluginLifecycleEventHandler;
                    lcEvents.AfterFeature += AfterFeaturePluginLifecycleEventHandler;
                }
                args.ObjectContainer.Resolve<IServiceCollectionFinder>();
            }
        }

        static void PrintRegisteredServiceInfo(IObjectContainer c)
        {
            var services = c.Resolve<IServiceCollection>();
            var servicesDescriptions = string.Join("\n", services
                                        .Select(x => $"{x.ServiceType.ToGenericTypeName()} => {(x.ImplementationType?.ToGenericTypeName() != null ? "factory" : "instance")} @ {x.Lifetime}")
                                        .OrderBy(x => x));
            Debug.WriteLine($"Autofac Root container created:\n{servicesDescriptions}");
        }

        private static void CustomizeFeatureDependenciesEventHandler(object sender, CustomizeFeatureDependenciesEventArgs args)
        {
            // container gets it's own set of services to register
            var services = new ServiceCollection();
            args.ObjectContainer.RegisterInstanceAs<IServiceCollection>(services);

            args.ObjectContainer.RegisterFactoryAs<IServiceProvider>(c =>
            {
                var rootAutofacContainer = c.Resolve<IContainer>();
                var scope = rootAutofacContainer.BeginLifetimeScope(FeatureScopeTag, x => x.Populate(services));
                c.RegisterInstanceAs(scope);

                // if there are registrations happening after SP resolution, flag them
                services.MakeReadOnly();

                var serviceProvider = new Autofac.Extensions.DependencyInjection.AutofacServiceProvider(scope);
                BindMappings.TryAdd(scope, args.ObjectContainer.Resolve<IContextManager>());
                ActiveServiceScopes.TryAdd(args.ObjectContainer.Resolve<FeatureContext>(), scope);

                return serviceProvider;
            });
        }

        private static void AfterFeaturePluginLifecycleEventHandler(object sender, RuntimePluginAfterFeatureEventArgs eventArgs)
        {
            if (ActiveServiceScopes.TryRemove(eventArgs.ObjectContainer.Resolve<FeatureContext>(), out var serviceScope))
            {
                BindMappings.TryRemove(serviceScope, out _);
                serviceScope.Dispose();
            }
        }

        private static void CustomizeScenarioDependenciesEventHandler(object sender, CustomizeScenarioDependenciesEventArgs args)
        {
            // container gets it's own set of services to register
            var services = new ServiceCollection();
            args.ObjectContainer.RegisterInstanceAs<IServiceCollection>(services);

            args.ObjectContainer.RegisterFactoryAs<IServiceProvider>(c =>
            {
                var rootAutofacContainer = c.Resolve<FeatureContext>()
                    .FeatureContainer
                    .Resolve<IServiceProvider>()
                    .GetRequiredService<ILifetimeScope>();
                var scope = rootAutofacContainer.BeginLifetimeScope(ScenarioScopeTag, x => x.Populate(services));
                c.RegisterInstanceAs(scope);

                // if there are registrations happening after SP resolution, flag them
                services.MakeReadOnly();

                var serviceProvider = new Autofac.Extensions.DependencyInjection.AutofacServiceProvider(scope);
                BindMappings.TryAdd(scope, args.ObjectContainer.Resolve<IContextManager>());
                ActiveServiceScopes.TryAdd(args.ObjectContainer.Resolve<ScenarioContext>(), scope);

                return serviceProvider;
            });
        }

        private static void AfterScenarioPluginLifecycleEventHandler(object sender, RuntimePluginAfterScenarioEventArgs eventArgs)
        {
            if (ActiveServiceScopes.TryRemove(eventArgs.ObjectContainer.Resolve<ScenarioContext>(), out var serviceScope))
            {
                BindMappings.TryRemove(serviceScope, out _);
                serviceScope.Dispose();
            }
        }

        private static void RegisterProxyBindings(IObjectContainer objectContainer, IServiceCollection services)
        {
            // Required for DI of binding classes that want container injections
            // While they can (and should) use the method params for injection, we can support it.
            // Note that in Feature mode, one can't inject "ScenarioContext", this can only be done from method params.

            // Bases on this: https://docs.specflow.org/projects/specflow/en/latest/Extend/Available-Containers-%26-Registrations.html
            // Might need to add more...

            services.AddSingleton(objectContainer);
            services.AddSingleton(sp => objectContainer.Resolve<IBindingAssemblyLoader>());
            services.AddSingleton(sp => objectContainer.Resolve<IBindingFactory>());
            services.AddSingleton(sp => objectContainer.Resolve<IBindingInvoker>());
            services.AddSingleton(sp => objectContainer.Resolve<IBindingRegistry>());
            services.AddSingleton(sp => objectContainer.Resolve<IErrorProvider>());
            services.AddSingleton(sp => objectContainer.Resolve<IRuntimeBindingSourceProcessor>());
            services.AddSingleton(sp => objectContainer.Resolve<IRuntimeConfigurationProvider>());
            services.AddSingleton(sp => objectContainer.Resolve<IRuntimePluginLoader>());
            services.AddSingleton(sp => objectContainer.Resolve<ISkeletonTemplateProvider>());
            services.AddSingleton(sp => objectContainer.Resolve<IStepDefinitionRegexCalculator>());
            services.AddSingleton(sp => objectContainer.Resolve<IStepDefinitionSkeletonProvider>());
            services.AddSingleton(sp => objectContainer.Resolve<IStepFormatter>());
            services.AddSingleton(sp => objectContainer.Resolve<IStepTextAnalyzer>());
            services.AddSingleton(sp => objectContainer.Resolve<ITestRunnerManager>());
            services.AddSingleton(sp => objectContainer.Resolve<ITestThreadExecutionEventPublisher>());
            services.AddSingleton(sp => objectContainer.Resolve<ITestTracer>());
            services.AddSingleton(sp => objectContainer.Resolve<ITraceListener>());
            services.AddSingleton(sp => objectContainer.Resolve<ITraceListenerQueue>());
            services.AddSingleton(sp => objectContainer.Resolve<IUnitTestRuntimeProvider>());
            services.AddTransient(sp =>
            {
                var container = BindMappings.TryGetValue(sp.GetRequiredService<ILifetimeScope>(), out var ctx)
                    ? ctx.ScenarioContext?.ScenarioContainer ??
                      ctx.FeatureContext?.FeatureContainer ??
                      ctx.TestThreadContext?.TestThreadContainer ??
                      objectContainer
                    : objectContainer;

                return container.Resolve<ISpecFlowOutputHelper>();
            });

            services.AddTransient(sp => BindMappings[sp.GetRequiredService<ILifetimeScope>()]);
            services.AddTransient(sp => BindMappings[sp.GetRequiredService<ILifetimeScope>()].FeatureContext);
            services.AddTransient<IFeatureContext>(sp => BindMappings[sp.GetRequiredService<ILifetimeScope>()].FeatureContext);
            services.AddTransient(sp => BindMappings[sp.GetRequiredService<ILifetimeScope>()].ScenarioContext);
            services.AddTransient<IScenarioContext>(sp => BindMappings[sp.GetRequiredService<ILifetimeScope>()].ScenarioContext);
            services.AddTransient(sp => BindMappings[sp.GetRequiredService<ILifetimeScope>()].TestThreadContext);
            services.AddTransient(sp => BindMappings[sp.GetRequiredService<ILifetimeScope>()].TestThreadContext.TestThreadContainer.Resolve<ITestRunner>());
            services.AddTransient(sp => BindMappings[sp.GetRequiredService<ILifetimeScope>()].TestThreadContext.TestThreadContainer.Resolve<ITestExecutionEngine>());
            services.AddTransient(sp => BindMappings[sp.GetRequiredService<ILifetimeScope>()].TestThreadContext.TestThreadContainer.Resolve<IStepArgumentTypeConverter>());
            services.AddTransient(sp => BindMappings[sp.GetRequiredService<ILifetimeScope>()].TestThreadContext.TestThreadContainer.Resolve<IStepDefinitionMatchService>());
        }
    }
}
