using Microsoft.Extensions.DependencyInjection;

namespace NativeWaves.Reqnroll.AutofacServiceProvider
{
    public interface IServiceCollectionFinder
    {
        IServiceCollection GetServiceCollection();
    }
}
