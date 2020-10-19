using Castle.DynamicProxy;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using Volo.Abp;
using Volo.Abp.Castle.DynamicProxy;
using Volo.Abp.Validation;

namespace Vls.Abp.Examples.Client
{
    public static class ServiceCollectionNatsClientProxyExtensions
    {
        private static readonly ProxyGenerator _proxyGeneratorInstance = new ProxyGenerator();

        public static IServiceCollection AddNatsClientProxies(
            [NotNull] this IServiceCollection services,
            [NotNull] Assembly assembly,
            [NotNull] string remoteServiceConfigurationName = "Default",
            bool asDefaultServices = true)
        {
            Check.NotNull(services, nameof(assembly));

            var serviceTypes = assembly.GetTypes().Where(IsSuitableForDynamicClientProxying).ToArray();

            foreach (var serviceType in serviceTypes)
            {
                services.AddNatsClientProxy(
                    serviceType,
                    remoteServiceConfigurationName,
                    asDefaultServices
                );
            }

            return services;
        }

        public static IServiceCollection AddNatsClientProxy<T>(
            [NotNull] this IServiceCollection services,
            [NotNull] string remoteServiceConfigurationName = "Default",
            bool asDefaultService = true)
        {
            return services.AddNatsClientProxy(
                typeof(T),
                remoteServiceConfigurationName,
                asDefaultService
            );
        }

        public static IServiceCollection AddNatsClientProxy(
            [NotNull] this IServiceCollection services,
            [NotNull] Type type,
            [NotNull] string remoteServiceConfigurationName = "Default",
            bool asDefaultService = true)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(type, nameof(type));
            Check.NotNullOrWhiteSpace(remoteServiceConfigurationName, nameof(remoteServiceConfigurationName));

            services.Configure<AbpNatsClientOptions>(options =>
            {
                options.NatsClientProxies[type] = new NatsClientProxyConfig(type, remoteServiceConfigurationName);
            });

            var interceptorType = typeof(NatsProxyInterceptor<>).MakeGenericType(type);
            services.AddTransient(interceptorType);

            var interceptorAdapterType = typeof(AbpAsyncDeterminationInterceptor<>).MakeGenericType(interceptorType);

            var validationInterceptorAdapterType =
                typeof(AbpAsyncDeterminationInterceptor<>).MakeGenericType(typeof(ValidationInterceptor));

            if (asDefaultService)
            {
                services.AddTransient(
                    type,
                    serviceProvider => _proxyGeneratorInstance
                        .CreateInterfaceProxyWithoutTarget(
                            type,
                            (IInterceptor)serviceProvider.GetRequiredService(validationInterceptorAdapterType),
                            (IInterceptor)serviceProvider.GetRequiredService(interceptorAdapterType)
                        )
                );
            }

            services.AddTransient(
                typeof(INatsClientProxy<>).MakeGenericType(type),
                serviceProvider =>
                {
                    var service = _proxyGeneratorInstance
                        .CreateInterfaceProxyWithoutTarget(
                            type,
                            (IInterceptor)serviceProvider.GetRequiredService(validationInterceptorAdapterType),
                            (IInterceptor)serviceProvider.GetRequiredService(interceptorAdapterType)
                        );

                    return Activator.CreateInstance(
                        typeof(INatsClientProxy<>).MakeGenericType(type),
                        service
                    );
                });

            return services;
        }

        private static bool IsSuitableForDynamicClientProxying(Type type)
        {
            //TODO: Add option to change type filter

            return type.IsInterface
                && type.IsPublic
                && !type.IsGenericType
                && typeof(IRemoteService).IsAssignableFrom(type);
        }
    }
}
