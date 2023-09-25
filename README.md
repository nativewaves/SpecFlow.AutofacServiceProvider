# SpecFlow.DependencyInjection

SpecFlow plugin that enables to use Autofac.Extensions.DependencyInjection for resolving test dependencies.
The ServiceCollection can be extended using the SpecFlow scenario-hooks. AutoFac is used for the IServiceProvider implementation and the object lifecycle handling.

Currently supports:
* [SpecFlow v3.9.74](https://www.nuget.org/packages/SpecFlow/3.9.74) or above
* [Microsoft.Extensions.DependencyInjection v7.0.0](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/7.0.0) or above

Based on [Autofac.Extensions.DependencyInjection](https://www.nuget.org/packages/Autofac.Extensions.DependencyInjection).

## Usage

Install plugin from NuGet into your SpecFlow project.

```powershell
PM> Install-Package NativeWaves.SpecFlow.AutofacServiceProvider
```

Create a static method in your SpecFlow project that returns a `Microsoft.Extensions.DependencyInjection.IServiceCollection` and tag it with the `[RootDependencies]` attribute. 
Configure your root dependencies within this method, which will be available for all Features/Scenarios.
Each step definition can implement a static public method tagged with a `[BeforeFeature]` or `[BeforeScenario]` attribute, that has a `IServiceCollection` as its parameter. In this 
service-collection you can register services which which take effect on a feature or scenario level. A `IServiceProvider` created for a `ScenarioContext` has access to all registrations inside the `[RootDependencies]` or services registered inside the `IServiceCollection`. Objects obey the lifetime-scope of autofac. Singletons life in the `Root-Scope`, Scoped services on a FeatureLevel inside the `Feature-Scope`, and all other scoped or transient services on a scenario-level inside the `Scenario-Scope`.
After a Scenario has finished, all object created for the scenario will be disposed. After a feature ended, all objects scoped to the feature will be disposed. After the test-run, all other services/singletons will be disposed.

Step definition classes (i.e. classes with the SpecFlow `[Binding]` attribute) are automatically added to the service collection.

A typical dependency builder method looks like this:

```csharp
[RootDependencies]
public static IServiceCollection CreateServices()
{
    var services = new ServiceCollection();
    
    services.AddSingleton<IMyService, MyService>();
    services.AddTransient<IMyTransient, MyTransient>();

    return services;
}

// binds a different 'IMyTransient' for @Special scenarios/features
[Binding, Scope(Tag = "Special")]
public class MySpecialBindingSteps
{
    [BeforeFeature]
    public static SpecialRegistrations(IServiceCollection services)
    {
        // provide a different implementation
        services.AddScoped<IMyTransient, MySpecialTransient>(); // singleton for the life-time of the feature
    }
}

[Binding]
public class ScenarioSteps
{
    [BeforeScenario]
    public static Setup(IServiceCollection services)
    {
        // provide a different implementation
        services.AddTransient<IMyOtherService, MyOtherService>(); // creation per scenario-dependency
        service.AddScoped<MyConcreteService>(); // singleton for the life-time of the scenario
    }
}
```

