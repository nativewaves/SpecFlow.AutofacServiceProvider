# Reqnroll.DependencyInjection

Reqnroll plugin that enables to use Autofac.Extensions.DependencyInjection for resolving test dependencies.
The ServiceCollection can be extended using the Reqnroll scenario-hooks. AutoFac is used for the IServiceProvider implementation and the object lifecycle handling.

Currently supports:
* [Reqnroll v3.9.74](https://www.nuget.org/packages/Reqnroll/3.9.74) or above
* [Microsoft.Extensions.DependencyInjection v7.0.0](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/7.0.0) or above

Based on [Autofac.Extensions.DependencyInjection](https://www.nuget.org/packages/Autofac.Extensions.DependencyInjection).

## Usage

Install plugin from NuGet into your Reqnroll project.

```powershell
PM> Install-Package NativeWaves.Reqnroll.AutofacServiceProvider
```

Create a static method in your Reqnroll project flagged with the `[RootDependencies]` attribute, which should return an instance of `Microsoft.Extensions.DependencyInjection.IServiceCollection`. Within this method, you'll configure your root dependencies, making them accessible to all Features and Scenarios.

For each step definition, you can define a static public method tagged with either `[BeforeFeature]` or `[BeforeScenario]` which takes an `IServiceCollection` as a parameter. Within this service collection, you can register services that will impact the DI on a scenario level.

The IServiceProvider created for a ScenarioContext will have access to all the registrations made within the `[RootDependencies]`` or any services registered within the `IServiceCollection``. Objects adhere to the lifetime scope defined by Autofac, with Singletons residing in the Root Scope, Scoped services at the Feature Level within the Feature Scope, and all other Scoped or Transient services at the Scenario Level within the Scenario Scope.

Once a Scenario finishes, all objects created in the scenario life-time-scope will be disposed. Similarly, after a feature ends, all objects scoped to the feature life-time-scope will be disposed. Eventually all other services and singletons will be disposed after the test run.

All Step definition classes, marked with the Reqnroll `[Binding]` attribute, are automatically imported into the root service collection.

A typical dependency builder method is structured as follows:

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

