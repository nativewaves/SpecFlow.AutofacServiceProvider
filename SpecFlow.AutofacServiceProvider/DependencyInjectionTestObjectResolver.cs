using Autofac.Core;
using BoDi;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using TechTalk.SpecFlow.Infrastructure;

namespace NativeWaves.SpecFlow.AutofacServiceProvider
{
    /* TODO
       If SpecFlow will add an "IObjectContainer.IsRegistered(Type type)" method next to the existing "IsRegistered<T>()"
       We can remove most of the code here!
     */
    public class DependencyInjectionTestObjectResolver : ITestObjectResolver
    {
        private static bool IsConcreteType(Type type) => type.IsClass && !type.IsAbstract && !type.IsInterface;

        // Can remove if IsRegistered(Type type) exists
        private static readonly ConcurrentDictionary<Type, MethodInfo> IsRegisteredMethodInfoCache =
            new ConcurrentDictionary<Type, MethodInfo>();

        // Can remove if IsRegistered(Type type) exists
        private static readonly MethodInfo IsRegisteredMethodInfo = typeof(DependencyInjectionTestObjectResolver)
            .GetMethod(nameof(IsRegistered), BindingFlags.Instance | BindingFlags.Public);

        // Can remove if IsRegistered(Type type) exists
        private static MethodInfo CreateGenericMethodInfo(Type t) => IsRegisteredMethodInfo.MakeGenericMethod(t);

        public object ResolveBindingInstance(Type bindingType, IObjectContainer container)
        {
            // Can remove if IsRegistered(Type type) exists
            var methodInfo = IsRegisteredMethodInfoCache.GetOrAdd(bindingType, CreateGenericMethodInfo);
            var bodiRegistered = (bool)methodInfo.Invoke(this, new object[] { container });

            return bodiRegistered
                ? container.Resolve(bindingType)
                : container.Resolve<IServiceProvider>().GetRequiredService(bindingType);
        }

        public bool IsRegistered<T>(IObjectContainer container)
        {
            if (container.IsRegistered<T>())
            {
                return true;
            }

            // IsRegistered is not recursive, it will only check the current container
            if (container is ObjectContainer c && c.BaseContainer != null)
            {
                return IsRegistered<T>(c.BaseContainer);
            }

            return false;
        }
    }
}
