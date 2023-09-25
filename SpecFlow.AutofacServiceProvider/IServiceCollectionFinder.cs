using Microsoft.Extensions.DependencyInjection;

namespace NativeWaves.SpecFlow.AutofacServiceProvider
{
    public interface IServiceCollectionFinder
    {
        IServiceCollection GetServiceCollection();
    }
}
